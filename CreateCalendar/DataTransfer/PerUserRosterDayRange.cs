using CreateCalendar.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CreateCalendar.DataTransfer
{
    internal class PerUserRosterDayRange
    {
        public DateOnly Date { get; set; }
        public DateOnly EndDate { get; set; }
        public string Shift { get; set; }
        public string UId { get; set; }
        public IList<OthersDay> Others { get; set; }
    }
}
