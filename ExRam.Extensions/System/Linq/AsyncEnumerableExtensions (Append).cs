using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> Append<T>(this IAsyncEnumerable<T> enumerable, T value)
        {
            Contract.Requires(enumerable != null);

            return enumerable.Concat(AsyncEnumerable.Return(value));
        }
    }
}
