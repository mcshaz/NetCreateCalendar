using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CreateCalendar.CreateIcs
{
    internal class DayRangeToIcsMapper
    {
        private readonly CalendarWriter _writer;
        private readonly DateTime _dateStamp;
        public DayRangeToIcsMapper(Stream stream) { 
            _writer = new CalendarWriter(stream);
            _dateStamp = DateTime.UtcNow;
            Ids = new List<string>();
        }
        public IList<string> Ids { get; private set; }
        public async Task MapRange(PerUserRosterDayRange dayRange)
        {
            await _writer.WriteLineRaw("BEGIN:VEVENT");
            await _writer.WriteProperty("DTSTAMP", _dateStamp);
            await _writer.WriteProperty("UID", dayRange.UId);
            await _writer.WriteProperty("DTSTART", dayRange.Date);
            // note calendar spec is EXCLUSIVE of the end date
            if (dayRange.Date != dayRange.EndDate)
                await _writer.WriteProperty("DTEND", dayRange.EndDate.AddDays(1));
            await _writer.WriteProperty("SUMMARY", dayRange.Shift);
            await _writer.WriteLineRaw("LOCATION:SCUH");
            await _writer.WriteProperty("DESCRIPTION", cf => GetDescription(cf, dayRange.Others));
            await _writer.WriteLineRaw("END:VEVENT");
            _writer.Flush();
        }

        private static void GetDescription(ContentFolder cf, IList<OthersDay> othersDays)
        {
            if (othersDays.Count == 1)
            {
                foreach (var e in othersDays[0].OtherEmployeesAvailable)
                    cf.WriteLine(e.ShiftName, " - ", e.EmployeeName);
            }
            else
            {
                foreach (var od in othersDays)
                {
                    const string dateFormat = "ddd mmm d";
                    if (od.Date == od.EndDate)
                    {
                        cf.WriteLine(od.Date.ToString(dateFormat));
                    }
                    else
                    {
                        cf.WriteLine(od.Date.ToString(dateFormat), " - ", od.EndDate.ToString(dateFormat));
                    }
                    foreach (var e in othersDays[0].OtherEmployeesAvailable)
                        cf.WriteLine("\t", e.ShiftName, " - ", e.EmployeeName);
                }
            }
        }
    }
}
