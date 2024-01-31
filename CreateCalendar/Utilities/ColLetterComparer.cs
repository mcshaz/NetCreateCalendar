using System;
using System.Collections.Generic;

namespace CreateCalendar.Utilities
{
    internal class ColLetterComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            
            var compare = y.Length - x.Length;
            return compare == 0
                ? StringComparer.OrdinalIgnoreCase.Compare(x, y)
                : compare;
        }
        public static ColLetterComparer Instance = new ColLetterComparer();
    }
}
