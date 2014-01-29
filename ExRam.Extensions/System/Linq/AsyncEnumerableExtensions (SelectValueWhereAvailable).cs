using System.Collections.Generic;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> SelectValueWhereAvailable<T>(this IAsyncEnumerable<Maybe<T>> enumerable)
        {
            return enumerable
                .Where(x => x.HasValue)
                .Select(x => x.Value);
        }
    }
}
