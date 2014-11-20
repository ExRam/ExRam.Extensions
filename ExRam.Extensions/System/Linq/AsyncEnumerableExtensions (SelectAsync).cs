// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, Task<TResult>> selector)
        {
            return enumerable
                .SelectAsync((x, ct) => selector(x)); 
        }

        public static IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, Task<TResult>> selector)
        {
            return AsyncEnumerable2.Create(() =>
            {
                var baseEnumerator = enumerable.GetEnumerator();

                return AsyncEnumeratorEx.Create(async (ct) =>
                {
                    var moved = await baseEnumerator.MoveNext(ct);

                    if (moved)
                        return await selector(baseEnumerator.Current, ct);

                    return Maybe<TResult>.Null;
                }, baseEnumerator.Dispose);
            });
        }
    }
}
