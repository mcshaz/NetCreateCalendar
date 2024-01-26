using CreateCalendar.CreateIcs;
using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.ReadCal
{
    internal static class ConglomerateExisting
    {
        public static async Task Conglomerate(IEnumerable<ApptDetails> oldAppts, 
            IEnumerable<PerUserRosterDayRange> contextAppts, 
            CalendarWriter cw,
            OldAppointmentOptions oldAppointments)
        {
            /* 
             * divide into:
             * - old cancelled & not reactivated - i.e. not in new (keep as is) 
             * - present on old but missing on newly created (make cancelled but increment sequence)
             * - present on both (make sure sequence is increments but created date kept)
             * - present only on newly created - keep, do nothing
             */
            var oldById = oldAppts
                .ToDictionary(a => a.Uid);
            foreach (var ca in contextAppts)
            {
                if (oldById.TryGetValue(ca.UId, out ApptDetails ad))
                {
                    oldById.Remove(ca.UId);
                    ca.Sequence = ad.Sequence + 1;
                    ca.Created = ad.Created;
                }
            }
            if (oldAppointments == OldAppointmentOptions.StatusCancelled)
            {
                var utcNow = DateTime.UtcNow;
                foreach (var c in oldById.Values)
                {
                    if (!c.IsCancelled)
                    {
                        c.IsCancelled = true;
                        ++c.Sequence;
                        c.LastModified = utcNow;
                    }
                    c.DtStamp = utcNow;
                    await c.Write(cw);
                }
            }
        }
    }
}
