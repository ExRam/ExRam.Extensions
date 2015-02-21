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

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> Gate<T>(this IAsyncEnumerable<T> enumerable, Func<CancellationToken, Task> gateTaskFunction)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(gateTaskFunction != null);

            return AsyncEnumerable
                .Using(
                    enumerable.GetEnumerator,
                    e => AsyncEnumerable
                        .Repeat(Unit.Default)
                        .SelectMany(_ => AsyncEnumerable
                            .Using(
                                () => new CancellationDisposable(),
                                cts => gateTaskFunction(cts.Token)
                                    .ToAsyncEnumerable()))
                        .SelectMany((_, ct) => e.MoveNextAsMaybe(ct))
                        .TakeWhile(x => x.HasValue)
                        .Select(x => x.Value));
        }
    }
}