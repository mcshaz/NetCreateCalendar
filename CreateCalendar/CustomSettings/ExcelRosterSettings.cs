﻿using System.Collections.Generic;

namespace CreateCalendar.CustomSettings
{
    public class ExcelRosterSettings : IExcelFileSettings
    {
        public string Url { get; set; }
        public int DateCol { get; set; } = 1;
        public string EmployeeNamesBeneath { get; set; } = "Roster";
        public int? DateCommentsCol { get; set; }
        public IList<string> SpecialShiftHeaders { get; set; } = new string[0];

        public IList<string> IgnoreShifts { get; set; } = new string[0];

        public IList<string> NonAvailableShifts { get; set; } = new string[0];
    }
}