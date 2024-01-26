using System;
using System.Collections.Generic;

namespace CreateCalendar
{
    internal static class LinqExtensions
    {
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, params T[] appended)
        {
            foreach (var e in enumerable)
            {
                yield return e;
            }
            foreach (var a in appended)
            {
                yield return a;
            }
        }
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> enumerable)
        {
            var e = enumerable.GetEnumerator();
            if (e.MoveNext())
            {
                var last = e.Current;
                while (e.MoveNext())
                {
                    yield return last;
                    last = e.Current;
                }
            }
        }
        public static IDictionary<K,V> RemoveMany<K, V>(this IDictionary<K, V> dict, IEnumerable<K> enumerable)
        {
            foreach (var e in enumerable) { 
                dict.Remove(e); 
            }
            return dict;
        }
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
