using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CreateCalendar.ReadCal
{
    internal static class ExtractCalDetails
    {
        internal static async Task<CalDetails> Extract(StreamReader reader)
        {
            var calDetails = new CalDetails
            {
                Appointments = new List<ApptDetails>(),
            };
            const string beginAppt = "BEGIN:VEVENT";
            const string endAppt = "END:VEVENT";
            const string uid = "UID:";
            const string dtStamp = "DTSTAMP:";
            const string status = "STATUS:";
            string nextLine;
            while ((nextLine = await reader.ReadLineAsync()) != null && nextLine != beginAppt) { }
            if (nextLine != null)
            {
                var apptDetails = new ApptDetails
                {
                    Content = new List<string> { beginAppt }
                };
                while ((nextLine = await reader.ReadLineAsync()) != null && nextLine != endAppt) {
                    apptDetails.Content.Add(nextLine);
                    if (nextLine.StartsWith(uid))
                    {
                        apptDetails.Uid = uid.Substring(uid.Length);
                    }
                    else if(nextLine.StartsWith(dtStamp))
                    {
                        apptDetails.LastUpdated = ICalDtTimeToNet(uid.Substring(dtStamp.Length));
                    }
                    else if(nextLine.StartsWith(status)) 
                    {
                        apptDetails.IsCancelled = uid.Substring(status.Length) == "CANCELLED";
                    }
                }
                apptDetails.Content.Add(endAppt);
                calDetails.Appointments.Add(apptDetails);
            }
            return calDetails;
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
