
using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    public interface IExcelFileSettings
    {
        int DateCol { get; }
        string EmployeeNamesBeneath { get; }
        int? DateCommentsCol { get; }
        IList<string> SpecialShiftHeaders { get; }
        IList<string> IgnoreShifts { get; }
        IList<string> NonAvailableShifts { get; }
    }
}
