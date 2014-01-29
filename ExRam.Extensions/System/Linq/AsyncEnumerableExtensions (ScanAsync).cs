using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TAccumulate> ScanAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulator)
        {
            Contract.Requires(source != null);
            Contract.Requires(accumulator != null);

            return AsyncEnumerable2.Create(() =>
            {
                var acc = seed;
                var e = source.GetEnumerator();

                return AsyncEnumeratorEx.Create(
                    async (ct) =>
                    {
                        var maybeItem = await e.MoveNextAsMaybe(ct);

                        if (maybeItem.HasValue)
                            return acc = await accumulator(acc, maybeItem.Value);

                        return Maybe<TAccumulate>.Null;
                    },
                    e.Dispose);
            });
        }
    }
}