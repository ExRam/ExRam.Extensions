// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TAccumulate> ScanAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulator)
        {
            Contract.Requires(source != null);
            Contract.Requires(accumulator != null);

            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var acc = seed;
                    var e = source.GetEnumerator();

                    return AsyncEnumerable.CreateEnumerator(
                        ct => e
                            .MoveNext(ct)
                            .Then(result => result
                                ? accumulator(acc, e.Current, ct)
                                    .Then(newAcc =>
                                    {
                                        acc = newAcc;

                                        return true;
                                    })
                                : Task.FromResult(false)),
                        () => acc,
                        e.Dispose);
                });
        }
    }
}