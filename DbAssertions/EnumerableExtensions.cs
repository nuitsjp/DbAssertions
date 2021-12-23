using System.Collections.Generic;
using System.Linq;

namespace DbAssertions
{
    internal static class EnumerableExtensions
    {
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T>? source) => !source?.Any() ?? true;
    }
}