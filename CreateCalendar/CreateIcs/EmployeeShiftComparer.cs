using CreateCalendar.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.CreateIcs
{
    internal class EmployeeShiftComparer : IEqualityComparer<EmployeeShift>
    {
        public bool Equals(EmployeeShift? x, EmployeeShift? y)
        {
            if (x == null || y == null) return x == y;
            return x.EmployeeName == y.EmployeeName && x.ShiftName == y.ShiftName;
        }

        public int GetHashCode(EmployeeShift obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + obj.ShiftName.GetHashCode();
                return hash * 23 + obj.EmployeeName.GetHashCode();
            }
        }
    }
}
