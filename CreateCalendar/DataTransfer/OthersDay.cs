using System;
using System.Collections.Generic;

namespace CreateCalendar.DataTransfer
{
    internal class OthersDay : IDateOnlyRange
    {
        public DateOnly Date { get; set; }
        public DateOnly EndDate { get; set; }
        public IEnumerable<EmployeeShift> OtherEmployeesAvailable { get; set; }
    }
}
