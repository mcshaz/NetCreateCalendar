using AngleSharp.Common;
using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using CreateCalendar.GoogleDrive;
using CreateCalendar.ProcessXlCalendar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CreateCalendar
{
    internal class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Initialising...");
            using (var host = Startup.HostBuilder())
            {
                await host.StartAsync();
                Console.WriteLine("Logging in (directing to a browser)...");
                using (var scope = host.Services.CreateScope())
                {
                    var appOpts = scope.ServiceProvider.GetRequiredService<IOptions<List<CreateCalendarFileSettings>>>();
                    // Obtain a PnP Context factory
                    var pnpContextFactory = scope.ServiceProvider.GetRequiredService<IPnPContextFactory>();
                    // Use the PnP Context factory to get a PnPContext for the given configuration
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    var opts = scope.ServiceProvider.GetRequiredService<IOptions<CreateCalendarSettings>>().Value;

                    var calData = new List<ProcessedXlFileDetails>();
                    using (var context = await pnpContextFactory.CreateAsync("SiteToWorkWith"))
                    {
                        foreach (var opt in opts.Calendars)
                        {
                            var file = await context.Web.GetFileByServerRelativeUrlAsync(opt.ExcelRoster.Url);

                            var path = file.ServerRelativeUrl.Split('/');
                            var xlRosterFilename = path.Last();
                            xlRosterFilename = xlRosterFilename.Substring(0, xlRosterFilename.LastIndexOf('.'));

                            FileStream localStream = null;
                            string tfn = null;
                            try
                            {
                                using (var spf = await file.GetContentAsync(true))
                                {
                                    logger.LogTrace("stream downloaded");
                                    tfn = Path.GetTempFileName();
                                    localStream = File.Open(tfn, FileMode.Open);
                                    spf.CopyTo(localStream);
                                }

                                var dataReaderLogger = scope.ServiceProvider.GetRequiredService<ILogger<XlDataReader>>();

                                var xlDataReader = new XlDataReader(
                                    dataReaderLogger,
                                    opt.ExcelRoster
                                );

                                calData.Add(new ProcessedXlFileDetails {
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
                    using (var googleDrive = await GoogleDriveIcsFiles.Create(opts.GoogleUser))
                    {
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
                                    if (allFiles.TryGetValue(fileName, out Google.Apis.Drive.v3.Data.File f))
                                    {
                                        var ms = new MemoryStream((int)f.Size);
                                        await googleDrive.WriteToStream(f.Id, ms);
                                        return ms;
                                    }
                                    return null;
                                },
                                async (fileName, stream) =>
                                {
                                    Uri findFile;
                                    if (allFiles.TryGetValue(fileName, out Google.Apis.Drive.v3.Data.File f))
                                    {
                                        findFile = await googleDrive.UpdateFile(f, stream);
                                    }
                                    else {
                                        findFile =await googleDrive.CreateFile(fileName, stream);
                                    }
                                    Console.WriteLine($"{fileName} {findFile}");
                                }
                            );
                        }
                    }
                }
            }
            Console.WriteLine("Complete - press [enter] to close");
            Console.ReadLine();
        }
    }
}
