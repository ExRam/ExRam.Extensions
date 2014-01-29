using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static Task<Maybe<T>> TryFirst<T>(this IAsyncEnumerable<T> enumerable)
        {
            return enumerable.TryFirst(CancellationToken.None);
        }

        public static async Task<Maybe<T>> TryFirst<T>(this IAsyncEnumerable<T> enumerable, CancellationToken token)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                return await enumerator.MoveNextAsMaybe(token);
            }
        }
    }
}
