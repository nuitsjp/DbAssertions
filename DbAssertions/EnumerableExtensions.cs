using System.Collections.Generic;
using System.Linq;

namespace DbAssertions
{
    /// <summary>
    /// Enumerable extension method class.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Returns true if null or empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => source?.Any() ?? true;
    }
}