using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    public class ExcelRosterSettings : IExcelFileSettings
    {
        public required string Url { get; set; }
        public string DateCol { get; set; } = "A";
        public string EmployeeNamesBeneath { get; set; } = "Roster";
        public string SheetnamePattern { get; set; } = @"^20\d\d$";
        public string? DateCommentsCol { get; set; }
        public IList<string> SpecialShiftHeaders { get; set; } = new List<string>();

        public IList<string> IgnoreShifts { get; set; } = new List<string>();

        public IList<string> NonAvailableShifts { get; set; } = new List<string>();
    }
}
