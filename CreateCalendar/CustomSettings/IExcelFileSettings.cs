
using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    public interface IExcelFileSettings
    {
        string DateCol { get; }
        string EmployeeNamesBeneath { get; }
        string SheetnamePattern { get; }
        string? DateCommentsCol { get; }
        IList<string> SpecialShiftHeaders { get; }
        IList<string> IgnoreShifts { get; }
        IList<string> NonAvailableShifts { get; }
    }
}
