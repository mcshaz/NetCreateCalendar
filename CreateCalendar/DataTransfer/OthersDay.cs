using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.DataTransfer
{
    internal class OthersDay
    {
        public DateOnly Date { get; set; }
        public DateOnly EndDate { get; set; }
        public IEnumerable<EmployeeShift> OtherEmployeesAvailable { get; set; }
    }
}
