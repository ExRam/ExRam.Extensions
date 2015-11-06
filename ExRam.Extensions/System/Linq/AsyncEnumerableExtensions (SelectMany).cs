// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, Task<TResult>> selector)
        {
            return AsyncEnumerableExtensions.Create(
                () =>
                {
                    var current = default(TResult);
                    var e = enumerable.GetEnumerator();

                    return AsyncEnumerableExtensions.Create(
                        ct => e
                            .MoveNext(ct)
                            .Then(result => result
                                ? selector(e.Current, ct)
                                    .Then(newCurrent =>
                                    {
                                        current = newCurrent;

                                        return true;
                                    })
                                : AsyncEnumerableExtensions.FalseTask),
                        () => current,
                        e);
                });
        }

        public static IAsyncEnumerable<Unit> SelectMany<TSource>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, Task> selector)
        {
            return enumerable
                .SelectMany((x, ct) => selector(x, ct).AsUnitTask());
        }
    }
}
