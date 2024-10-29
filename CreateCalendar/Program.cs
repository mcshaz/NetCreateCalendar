// See https://aka.ms/new-console-template for more information
using CreateCalendar;
using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using CreateCalendar.GoogleDrive;
using CreateCalendar.ProcessXlCalendar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services;

Console.WriteLine("Initialising...");

using var host = Startup.HostBuilder();
await host.StartAsync();
Console.WriteLine("Logging in (directing to a browser)...");
using var scope = host.Services.CreateScope();
var appOpts = scope.ServiceProvider.GetRequiredService<IOptions<List<CreateCalendarFileSettings>>>();
// Obtain a PnP Context factory
var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();
// Use the PnP Context factory to get a PnPContext for the given configuration
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
var opts = scope.ServiceProvider.GetRequiredService<IOptions<CreateCalendarSettings>>().Value;

var calData = new List<ProcessedXlFileDetails>();

try
{
    var context = await pnpContextFactory.CreateAsync("SiteToWorkWith");
    foreach (var opt in opts.Calendars)
    {
        var file = await context.Web.GetFileByServerRelativeUrlAsync(opt.ExcelRoster.Url);

        var path = file.ServerRelativeUrl.Split('/');
        var xlRosterFilename = path.Last();
        xlRosterFilename = xlRosterFilename[..xlRosterFilename.LastIndexOf('.')];

        FileStream? localStream = null;
        string? tfn = null;
        try
        {
            using (var spf = await file.GetContentAsync(true))
            {
                logger.LogTrace("stream downloaded");
                tfn = Path.GetTempFileName();
                localStream = File.Open(tfn, FileMode.Open);
                spf.CopyTo(localStream);
            }

            // var dataReaderLogger = scope.ServiceProvider.GetRequiredService<ILogger<XlDataReader>>();

            var xlDataReader = new XlDataReader(
                opt.ExcelRoster
            );

            calData.Add(new ProcessedXlFileDetails
            {
                Settings = opt,
                Roster = xlDataReader.Process(localStream),
                UniqueId = file.UniqueId
            });

        }
        finally
        {
            localStream?.Dispose();
            if (tfn != null)
                File.Delete(tfn);
        }
    }
}
catch (HttpRequestException hre)
{
    if (hre.HttpRequestError == HttpRequestError.ConnectionError)
    {
        Console.WriteLine("Please check your internet connection");
    }
    return -1;
    // throw;
}
using var googleDrive = await GoogleDriveIcsFiles.Create(opts.GoogleUser);
foreach (var processedData in calData)
{
    googleDrive.WorkingFolderId = await googleDrive.GetFolderId(processedData.Settings.IcsFolder);
    var allFiles = (await googleDrive.ListFiles()).ToDictionary(f => f.Name);
    await ProcessAndOutput.ProcessCalendar(
        processedData.UniqueId,
        PerUserRosters.Create(processedData.Roster, processedData.Settings.ExcelRoster),
        processedData.Settings,
        async fileName =>
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            if (allFiles.TryGetValue(fileName, out Google.Apis.Drive.v3.Data.File f))
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                var ms = new MemoryStream(capacity: (int)f.Size);
#pragma warning restore CS8629 // Nullable value type may be null.
                await googleDrive.WriteToStream(f.Id, ms);
                return ms;
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            return null;
        },
        async (fileName, stream) =>
        {
            Uri findFile;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            if (allFiles.TryGetValue(fileName, out Google.Apis.Drive.v3.Data.File f))
            {
                findFile = await googleDrive.UpdateFile(f, stream);
            }
            else
            {
                findFile = await googleDrive.CreateFile(fileName, stream);
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            Console.WriteLine($"{fileName} {findFile}");
        }
    );
}

return 0;