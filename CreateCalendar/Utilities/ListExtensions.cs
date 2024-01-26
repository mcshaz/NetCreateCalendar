using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateCalendar.Utilities
{
    internal static class ListExtensions
    {
        public static T Pop<T>(this List<T> list)
        {
            var lastIndex = list.Count - 1;
            var returnVar = list[lastIndex];
            list.RemoveAt(lastIndex);
            return returnVar;
        }
    }
}
