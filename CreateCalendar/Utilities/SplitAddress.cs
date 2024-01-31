using System;

namespace CreateCalendar.Utilities
{
    internal readonly struct SplitAddress
    {
        public SplitAddress(string address)
        {
            int indx = 0;
            while (!char.IsDigit(address[indx])) { ++indx; }
            Column = address.Substring(0, indx);
            Row = int.Parse(address.Substring(indx));
        }
        public readonly int Row;
        public readonly string Column;
    }
}
