using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T> source)
        {
            Contract.Requires(source != null);

            return source.Where(t => !object.Equals(t, default(T)));
        }
    }
}
