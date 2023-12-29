using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.ProcessXlCalendar
{
    internal static class PerUserRosters
    {
        public static IEnumerable<PerUserRoster> Create(EveryoneRoster everyoneRoster)
        {
            foreach(var d in everyoneRoster.DailyRoster)
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
                            Shift = eShift.ShiftName, 
                            OtherEmployeesAvailable = d.Shifts.Where(s => s != eShift 
                                    && !string.Equals(s.ShiftName, "leave", StringComparison.OrdinalIgnoreCase))
                        }).ToList()
            }).ToList();
        }
    }
}
