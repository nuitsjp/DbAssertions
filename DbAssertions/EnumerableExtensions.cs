using System.Collections.Generic;
using System.Linq;
#if NET40
#else
using System;
#endif

namespace DbAssertions
{
    public static class EnumerableExtensions
    {
#if NET40
#else
        public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            IEnumerable<(T item, int index)> Implement()
            {
                var i = 0;
                foreach (var item in source)
                {
                    yield return (item, i);
                    ++i;
                }
            }

            return Implement();
        }
#endif
        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => !source?.Any() ?? true;
    }
}