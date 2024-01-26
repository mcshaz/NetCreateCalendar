using CreateCalendar.CustomSettings;
using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateCalendar.ProcessXlCalendar
{
    public static class PerUserRosters
    {
        public static IEnumerable<PerUserRoster> Create(EveryoneRoster everyoneRoster, IExcelFileSettings settings)
        {
            var nonAvailableShifts = new HashSet<string>(
                settings.NonAvailableShifts,
                StringComparer.OrdinalIgnoreCase
            );
            foreach (var d in everyoneRoster.DailyRoster)
            {
                d.Shifts = d.Shifts.OrderBy(s => s.ShiftName).ToList();
            }
            var returnVar = new List<PerUserRoster>();
            return everyoneRoster.Employees.Select(e => new PerUserRoster
            {
                EmployeeName = e,
                Days = (from d in everyoneRoster.DailyRoster
                        let eShift = d.Shifts.FirstOrDefault(s => s.EmployeeName == e)
                        where eShift != default
                        select new PerUserRosterDay
                        {
                            Date = d.Date,
                            DateComment = d.DateComment,
                            Shift = eShift.ShiftName,
                            OtherEmployeesAvailable = nonAvailableShifts.Contains(eShift.ShiftName)
                                ? Enumerable.Empty<EmployeeShift>()
                                : d.Shifts.Where(s => s != eShift && !nonAvailableShifts.Contains(s.ShiftName))
                        }).ToList()
            }).ToList();
        }
    }
}
