using System.Collections.Generic;

namespace CreateCalendar.DataTransfer
{
    public class PerUserRoster
    {
        public string EmployeeName { get; set; }
        public IEnumerable<PerUserRosterDay> Days { get; set; }
    }
}
