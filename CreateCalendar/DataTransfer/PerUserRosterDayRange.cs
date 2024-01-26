using CreateCalendar.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CreateCalendar.DataTransfer
{
    internal class PerUserRosterDayRange : IDateOnlyRange
    {
        public DateOnly Date { get; set; }
        public DateOnly EndDate { get; set; }
        public DateTime Created {  get; set; }
        public int Sequence { get; set; } = 1;
        public string Shift { get; set; }
        public string UId { get; set; }
        public IList<OthersDay> Others { get; set; }
        public IList<DateRangeComments> DateRangeComments { get; set; }
    }
    public class Organizer
    {
        public string Email { get; set; }
        public string CommonName { get; set; }
    }
}
