using System;
using System.Collections.Generic;

namespace CreateCalendar
{
    internal static class LinqExtensions
    {
        public static IEnumerable<T> AllAfter<T>(this IEnumerable<T> source, Func<T, bool> predicate) 
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext() && !predicate(enumerator.Current)) { }
                while (enumerator.MoveNext()) { yield return enumerator.Current; }
            }
        }
    }
}
