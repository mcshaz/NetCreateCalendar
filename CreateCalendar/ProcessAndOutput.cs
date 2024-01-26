using CreateCalendar.CreateIcs;
using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using CreateCalendar.ProcessXlCalendar;
using CreateCalendar.ReadCal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar
{
    public static class ProcessAndOutput
    {
        public static async Task ProcessCalendar(
            Guid rosterId,
            IEnumerable<PerUserRoster> employeesRosters,
            IEventSettings eventSettings,
            Func<string, Task<Stream>> getExistingIcs, 
            Func<string, MemoryStream, Task> writeIcs)
        {
            foreach (var pr in employeesRosters)
            {
                var coalesced = CoalesceDays.Coalesce(pr.Days, rosterId);

                var existingAppts = Enumerable.Empty<ApptDetails>();
                var fileName = string.Format(eventSettings.IcsFilename, pr.EmployeeName);
                const string icsExt = ".ics";
                if (!fileName.EndsWith(icsExt))
                    fileName += icsExt;
                using (var fs = await getExistingIcs(fileName))
                {
                    if (fs != null)
                        using (var sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            existingAppts = await ExtractCalDetails.Extract(sr);
                        }
                }
                using (var ms = new MemoryStream())
                {
                    var cw = new CalendarWriter(ms);
                    if (existingAppts.Any())
                        await ConglomerateExisting.Conglomerate(existingAppts, coalesced, cw);

                    var drw = new DayRangeToIcsMapper(cw, eventSettings);
                    foreach (var c in coalesced)
                    {
                        await drw.MapRange(c);
                    }
                    await cw.WriteEnd();
                    ms.Position = 0;
                    await writeIcs(fileName, ms);
                }
            }
        }
    }
}
