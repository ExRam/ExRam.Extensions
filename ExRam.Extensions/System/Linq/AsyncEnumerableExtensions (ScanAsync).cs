// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Monad;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TAccumulate> ScanAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulator)
        {
            Contract.Requires(source != null);
            Contract.Requires(accumulator != null);

            return AsyncEnumerable
                .Using(
                    source.GetEnumerator,
                    e =>
                    {
                        var acc = seed;

                        return AsyncEnumerable
                            .Repeat(Unit.Default)
                            .SelectMany(async (_, ct) =>
                            {
                                var maybeItem = await e.MoveNextAsMaybe(ct);

                                if (maybeItem.HasValue)
                                    return acc = await accumulator(acc, maybeItem.Value, ct);

                                return OptionStrict<TAccumulate>.Nothing;
                            })
                            .TakeWhile(x => x.HasValue)
                            .Select(x => x.Value);
                    });
        }
    }
}