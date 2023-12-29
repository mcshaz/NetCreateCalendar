using CreateCalendar.DataTransfer;
using CreateCalendar.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateCalendar.CreateIcs
{
    internal static class CoalesceDays
    {
        public static IEnumerable<PerUserRosterDayRange> Coalesce(IEnumerable<PerUserRosterDay> rosterDays, string rosterName)
        {
            var returnVar = new List<PerUserRosterDayRange>(rosterDays.Count());
            var lastShift = MapDayToRange(rosterDays.FirstOrDefault());
            if (lastShift != default)
            {
                var otherShiftComparer = new EmployeeShiftComparer();
                foreach (var d in rosterDays.Skip(1))
                {
                    if (d.Shift == lastShift.Shift && d.Date.DayNumber - lastShift.Date.DayNumber == 1)
                    {
                        lastShift.EndDate = d.Date;
                        var lastShiftOthers = lastShift.Others[lastShift.Others.Count - 1];
                        if (Enumerable.SequenceEqual(d.OtherEmployeesAvailable,
                            lastShiftOthers.OtherEmployeesAvailable, 
                            otherShiftComparer))
                        {
                            lastShiftOthers.EndDate = d.Date;
                        }
                        else
                        {
                            lastShift.Others.Add(new OthersDay
                            {
                                Date = d.Date,
                                EndDate = d.Date,
                                OtherEmployeesAvailable = d.OtherEmployeesAvailable
                            });
                        }
                    }
                    else
                    {
                        lastShift.UId = CreateUid(lastShift, rosterName);
                        returnVar.Add(lastShift);
                        lastShift = MapDayToRange(d);
                    }
                }
                returnVar.Add(lastShift);
            }
            return returnVar;
        }
        private static PerUserRosterDayRange MapDayToRange(PerUserRosterDay dayRoster)
        {
            if (dayRoster == null) { return null; }
            return new PerUserRosterDayRange
            {
                Date = dayRoster.Date,
                EndDate = dayRoster.Date,
                Shift = dayRoster.Shift,
                Others = new List<OthersDay> { 
                    new OthersDay { 
                        Date = dayRoster.Date, 
                        EndDate = dayRoster.Date, 
                        OtherEmployeesAvailable = dayRoster.OtherEmployeesAvailable,
                    } 
                }
            };
        }

        private static string CreateUid(PerUserRosterDayRange dayRangeRoster, string rosterName)
        {
            var hash = HashHelpers.CreateXxHash64(rosterName, dayRangeRoster.Date, dayRangeRoster.Shift);
            var base64Hash = Convert.ToBase64String(hash);
            // remove last '=' filling bits
            return $"{base64Hash.Substring(0, base64Hash.Length - 1)}@bmcsh.scuh";
        }
    }
}
