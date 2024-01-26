using CreateCalendar.CustomSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PnP.Core.Auth.Services.Builder.Configuration;
using PnP.Core.Services.Builder.Configuration;
using System.Collections.Generic;

namespace CreateCalendar
{
    internal static class Startup
    {
        public static IHost HostBuilder()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    var config = hostingContext.Configuration.GetSection("PnPCore");
                    // Add the PnP Core SDK library services
                    services.AddPnPCore();
                    // Add the PnP Core SDK library services configuration from the appsettings.json file
                    services.Configure<PnPCoreOptions>(config);
                    // Add the PnP Core SDK Authentication Providers
                    services.AddPnPCoreAuthentication();
                    // Add the PnP Core SDK Authentication Providers configuration from the appsettings.json file
                    services.Configure<PnPCoreAuthenticationOptions>(config);

                    var thisAppConfig = hostingContext.Configuration.GetSection("CreateCalendar");
                    services.Configure<CreateCalendarSettings>(thisAppConfig);
                })
                // Let the builder know we're running in a console
                .UseConsoleLifetime()
                // Add services to the container
                .Build();
            return host;
        }
    }
}
