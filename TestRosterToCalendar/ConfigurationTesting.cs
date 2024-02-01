using CreateCalendar;
using CreateCalendar.CustomSettings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PnP.Core.Services.Builder.Configuration;

namespace TestRosterToCal
{
    [TestClass]
    public class ConfigurationTesting
    {
        [TestMethod]
        public void JsonCanBeRead()
        {
            using var host = Startup.HostBuilder("appsettings.example.json");
            using var scope = host.Services.CreateScope();
            var pnpOpts = scope.ServiceProvider.GetService<IOptions<CreateCalendarSettings>>();
            Assert.IsNotNull(pnpOpts);
            Assert.IsNotNull(pnpOpts.Value);
            Assert.IsNotNull(pnpOpts.Value.Calendars);
            Assert.AreEqual(2, pnpOpts.Value.Calendars.Count);
            Assert.IsNotNull(pnpOpts.Value.Calendars[0].ExcelRoster);
            Assert.AreEqual("C", pnpOpts.Value.Calendars[0].ExcelRoster.DateCommentsCol);
            Assert.AreEqual("SCUH", pnpOpts.Value.Calendars[0].AppointmentKeyValues["LOCATION"]);
        }
    }
}
