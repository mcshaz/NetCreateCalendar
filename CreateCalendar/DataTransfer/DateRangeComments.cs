using System;

namespace CreateCalendar.DataTransfer
{
    internal class DateRangeComments : IDateOnlyRange
    {
        public DateOnly Date {  get; set; }
        public DateOnly EndDate { get; set; }
        public string DateComment { get; set; }
    }
}
