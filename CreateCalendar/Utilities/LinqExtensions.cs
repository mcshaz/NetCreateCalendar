using System;
using System.Collections.Generic;
using System.Linq;

namespace CreateCalendar
{
    internal static class LinqExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> values, T searchFor)
        {
            int i = 0;
            foreach (var v in values)
            {
                if (v.Equals(searchFor))
                {
                    return i;
                }
                ++i;
            }
            return -1;
        }
        public static T? FirstOrNull<T>(this IEnumerable<T> values, Func<T, bool> predicate) where T : struct
        {
            foreach (var v in values)
            {
                if (predicate(v))
                {
                    return v;
                }
            }
            return null;
        }
        public static T MaxBy<T>(this IEnumerable<T> values, IComparer<T> comparer)
        {
            return values.Aggregate(values.First(),(accum, v) => comparer.Compare(v, accum) > 1 ? v : accum);
        }
    }
}
