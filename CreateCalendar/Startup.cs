using CreateCalendar.CustomSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace CreateCalendar
{
    public static class Startup
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PnPCoreOptions))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PnPCoreAuthenticationOptions))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CreateCalendarSettings))]
        // [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PnPCoreSitesOptions))]
        // [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PnPCoreSiteOptions))]
        // [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Dictionary<string, PnPCoreSiteOptions>))]
        // [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(List<CreateCalendarFileSettings>))]
        // [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ExcelRosterSettings))]
        public static IHost HostBuilder(string? path = null)
        {
            var host = Host.CreateDefaultBuilder();
            if (path != null)
                host.ConfigureAppConfiguration(x => x.AddJsonFile(path));
            host.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    // using DSS Sharepoint Read - the Manifest of which can be found at:
                    // https://entra.microsoft.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Manifest/appId/bafef36f-1067-4238-9a75-d2f450740d38/isMSAApp~/false
                    var pnpConfig = hostingContext.Configuration.GetSection("PnPCore");
                    // Add the PnP Core SDK library services
                    services.AddPnPCore();
                    // Add the PnP Core SDK library services configuration from the appsettings.json file
                    services.Configure<PnPCoreOptions>(pnpConfig);
                    // Add the PnP Core SDK Authentication Providers
                    services.AddPnPCoreAuthentication();
                    // Add the PnP Core SDK Authentication Providers configuration from the appsettings.json file
                    services.Configure<PnPCoreAuthenticationOptions>(pnpConfig);

                    var thisAppConfig = hostingContext.Configuration.GetSection("CreateCalendar");
                    services.Configure<CreateCalendarSettings>(thisAppConfig);
                })
                // Let the builder know we're running in a console
                .UseConsoleLifetime();
            return host.Build();
        }
    }
}
