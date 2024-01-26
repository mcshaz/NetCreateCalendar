using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CreateCalendar.ReadCal
{
    internal static class ExtractCalDetails
    {
        internal static async Task<IEnumerable<ApptDetails>> Extract(StreamReader reader)
        {
            var appointments = new List<ApptDetails>();
            const string beginAppt = "BEGIN:VEVENT";

            string nextLine;
            // advance to first appointment
            while ((nextLine = await reader.ReadLineAsync()) != null && nextLine != beginAppt) { }
            while (nextLine == beginAppt)
            {
                var apptDetails = new ApptDetails();
                apptDetails.AddLine(nextLine);
                while ((nextLine = await reader.ReadLineAsync()) != null && apptDetails.AddLine(nextLine))
                { }
                appointments.Add(apptDetails);
                nextLine = await reader.ReadLineAsync();
            }
            return appointments;
        }

        public static DateTime ICalDtTimeToNet(string icalDtTime)
        {
            return new DateTime(
                int.Parse(icalDtTime.Substring(0, 4)),
                int.Parse(icalDtTime.Substring(4, 2)),
                int.Parse(icalDtTime.Substring(6, 2)),
                int.Parse(icalDtTime.Substring(9, 2)),
                int.Parse(icalDtTime.Substring(11, 2)),
                int.Parse(icalDtTime.Substring(13, 2)),
                icalDtTime.Length == 16
                    ? DateTimeKind.Utc
                    : DateTimeKind.Local
            );
        }
    }
}
