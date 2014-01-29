using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<Maybe<T>> TryWithTimeout<T>(this IAsyncEnumerable<T> enumerable, TimeSpan timeout)
        {
            return AsyncEnumerable2.Create(() =>
            {
                var enumerator = enumerable.GetEnumerator();

                return AsyncEnumeratorEx.Create(
                    async (ct) =>
                    {
                        var maybe = await enumerator.MoveNextAsMaybe(ct).TryWithTimeout(timeout).ConfigureAwait(false);
                        return ((maybe.HasValue) ? (maybe) : (Maybe<Maybe<T>>.Null));
                    },
                    enumerator.Dispose);
            });
        }
    }
}