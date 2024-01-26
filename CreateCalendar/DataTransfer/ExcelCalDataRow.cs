using System;
using System.Collections.Generic;

namespace CreateCalendar.DataTransfer
{
    public class ExcelCalDataRow
    {
        public DateOnly Date { get; set; }
        public string DateComment { get; set; }
        public IList<EmployeeShift> Shifts { get; set; }
    }
}
