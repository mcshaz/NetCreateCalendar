using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CreateCalendar.CreateIcs
{
    internal class DayRangeToIcsMapper
    {
        private readonly CalendarWriter _writer;
        private readonly DateTime _instantiatedUtc;
        private readonly IEventSettings _eventSettings;
        public DayRangeToIcsMapper(CalendarWriter writer, IEventSettings eventSettings) {
            _writer = writer;
            _instantiatedUtc = DateTime.UtcNow;
            _eventSettings = eventSettings;
        }
        public async Task MapRange(PerUserRosterDayRange dayRange)
        {
            await _writer.WriteLineRaw("BEGIN:VEVENT");
            await _writer.WriteProperty("UID", dayRange.UId);
            await _writer.WriteProperty("DTSTAMP", _instantiatedUtc);
            await _writer.WriteProperty("SEQUENCE", dayRange.Sequence);
            await _writer.WriteProperty("CREATED", dayRange.Created == default
                ? _instantiatedUtc
                : dayRange.Created);
            await _writer.WriteProperty("DTSTART", dayRange.Date);
            // note calendar spec is EXCLUSIVE of the end date
            if (dayRange.Date != dayRange.EndDate)
                await _writer.WriteProperty("DTEND", dayRange.EndDate.AddDays(1));
            await _writer.WriteProperty("SUMMARY", string.Format(_eventSettings.FormatShift, dayRange.Shift));
            if (dayRange.Others.Any() || dayRange.DateRangeComments.Any())
                await _writer.WriteProperty("DESCRIPTION", cf => GetDescription(cf, dayRange));
            foreach (var kv in _eventSettings.AppointmentKeyValues)
            {
                await _writer.WriteProperty(kv.Key, kv.Value);
            }
            await _writer.WriteLineRaw("END:VEVENT");
        }

        private static void GetDescription(ContentFolder cf, PerUserRosterDayRange dr)
        {
            foreach (var d in dr.DateRangeComments)
            {
                if (WriteDateRange(cf, dr, d))
                {
                    cf.WriteLine(" - ", d.DateComment);
                }
                else
                {
                    cf.WriteLine(d.DateComment);
                }
            }
            if (dr.DateRangeComments.Any()) 
                cf.WriteLine();
            foreach (var o in dr.Others)
            {
                string prepend;
                if (WriteDateRange(cf, dr, o))
                {
                    cf.WriteLine();
                    prepend = "\t";
                }
                else
                {
                    prepend = string.Empty;
                }
                foreach (var e in o.OtherEmployeesAvailable)
                {
                    cf.WriteLine(prepend, e.ShiftName, " - ", e.EmployeeName);
                }
            }
        }

        private static bool WriteDateRange(ContentFolder cf, IDateOnlyRange parent, IDateOnlyRange child)
        {
            if (parent.Date == child.Date && parent.EndDate == child.EndDate)
                return false;
            const string dateFormat = "ddd MMM d";
            if (child.Date == child.EndDate)
            {
                cf.Write(child.Date.ToString(dateFormat));
            }
            else
            {
                cf.Write(child.Date.ToString(dateFormat), " - ", child.EndDate.ToString(dateFormat));
            }
            return true;
        }
    }
}
