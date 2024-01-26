using CreateCalendar.DataTransfer;
using CreateCalendar.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateCalendar.CreateIcs
{
    internal static class CoalesceDays
    {
        public static IEnumerable<PerUserRosterDayRange> Coalesce(IEnumerable<PerUserRosterDay> rosterDays, Guid rosterId)
        {
            var returnVar = new List<PerUserRosterDayRange>(rosterDays.Count());
            var lastShift = MapDayToRange(rosterDays.FirstOrDefault());
            if (lastShift != default)
            {
                var otherShiftComparer = new EmployeeShiftComparer();
                foreach (var d in rosterDays.Skip(1))
                {
                    var isSequentiialDay = d.Date.DayNumber - lastShift.EndDate.DayNumber == 1;
                    if (isSequentiialDay && d.Shift == lastShift.Shift)
                    {
                        lastShift.EndDate = d.Date;
                        var lastShiftOthers = lastShift.Others.LastOrDefault();
                        if (lastShiftOthers != default
                            && d.Date.DayNumber - lastShiftOthers.EndDate.DayNumber == 1
                            && Enumerable.SequenceEqual(d.OtherEmployeesAvailable,
                                lastShiftOthers.OtherEmployeesAvailable, 
                                otherShiftComparer))
                        {
                            lastShiftOthers.EndDate = d.Date;
                        }
                        else if (d.OtherEmployeesAvailable.Any())
                        {
                            lastShift.Others.Add(new OthersDay
                            {
                                Date = d.Date,
                                EndDate = d.Date,
                                OtherEmployeesAvailable = d.OtherEmployeesAvailable
                            });
                        }
                        if (d.DateComment != null)
                        {
                            var lastShiftDateComments = lastShift.DateRangeComments.LastOrDefault();
                            if (lastShiftDateComments != null
                                && lastShiftDateComments.DateComment == d.DateComment
                                && d.Date.DayNumber - lastShiftDateComments.EndDate.DayNumber == 1)
                            {
                                lastShiftDateComments.EndDate = d.Date;
                            }
                            else
                            {
                                lastShift.DateRangeComments.Add(new DateRangeComments
                                {
                                    Date = d.Date,
                                    EndDate = d.Date,
                                    DateComment = d.DateComment,
                                });
                            }
                        }
                    }
                    else
                    {
                        lastShift.UId = CreateUid(rosterId, lastShift);
                        returnVar.Add(lastShift);
                        lastShift = MapDayToRange(d);
                    }
                }
                lastShift.UId = CreateUid(rosterId, lastShift);
                returnVar.Add(lastShift);
            }
            return returnVar;
        }
        private static PerUserRosterDayRange MapDayToRange(PerUserRosterDay dayRoster)
        {
            if (dayRoster == null) { return null; }
            var dateRangeComments = new List<DateRangeComments>();
            if (!string.IsNullOrEmpty(dayRoster.DateComment))
            {
                dateRangeComments.Add(new DateRangeComments
                {
                    DateComment = dayRoster.DateComment,
                    Date = dayRoster.Date,
                    EndDate = dayRoster.Date
                });
            }
            return new PerUserRosterDayRange
            {
                Date = dayRoster.Date,
                EndDate = dayRoster.Date,
                Shift = dayRoster.Shift,
                DateRangeComments = dateRangeComments,
                Others = new List<OthersDay> { 
                    new OthersDay { 
                        Date = dayRoster.Date, 
                        EndDate = dayRoster.Date, 
                        OtherEmployeesAvailable = dayRoster.OtherEmployeesAvailable,
                    } 
                }
            };
        }

        private static string CreateUid(Guid rosterId, PerUserRosterDayRange dayRangeRoster)
        {
            var hash = HashHelpers.CreateXxHash64(rosterId, dayRangeRoster.Shift, dayRangeRoster.Date, dayRangeRoster.EndDate);
            var base64Hash = Convert.ToBase64String(hash);
            // to do remove last '=' filling last byte
            return $"{base64Hash.Substring(0, base64Hash.Length - 1)}@bmcsh.scuh";
        }
    }
}
