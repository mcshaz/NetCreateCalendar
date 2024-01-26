using System;

namespace CreateCalendar.DataTransfer
{
    internal interface IDateOnlyRange
    {
        DateOnly Date { get; }
        DateOnly EndDate { get; }
    }
}
