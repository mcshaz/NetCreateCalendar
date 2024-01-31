using DocumentFormat.OpenXml.Spreadsheet;
using System;

namespace CreateCalendar.Utilities
{
    internal readonly struct AddressRange
    {
        public AddressRange(string range)
        {
            var addresses = range.Split(':');
            Start = new SplitAddress(addresses[0]);
            End = addresses.Length == 0
                ? Start
                : new SplitAddress(addresses[1]);
        }
        public readonly SplitAddress Start;
        public readonly SplitAddress End;
        public bool Contains(Cell cell) 
        {
            var cellAddress = new SplitAddress(cell.CellReference);
            return Start.Row <= cellAddress.Row && cellAddress.Row <= End.Row
                && ColLetterComparer.Instance.Compare(Start.Column, cellAddress.Column) <= 0
                && ColLetterComparer.Instance.Compare(cellAddress.Column, End.Column) <= 0;
        }
    }
}
