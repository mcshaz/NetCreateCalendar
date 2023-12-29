using System;
using System.Collections.Generic;

namespace CreateCalendar.DataTransfer
{
    public class ExcelCalDataRow: IComparable<ExcelCalDataRow>
    {
        public DateOnly Date { get; set; }
        public bool IsPublicHoliday { get; set; }
        public IList<EmployeeShift> Shifts { get; set; }
        public override int GetHashCode()
        {
            return Date.GetHashCode();
        }
        int IComparable<ExcelCalDataRow>.CompareTo(ExcelCalDataRow other)
        {
            return Date.CompareTo(other);
        }
    }
}
