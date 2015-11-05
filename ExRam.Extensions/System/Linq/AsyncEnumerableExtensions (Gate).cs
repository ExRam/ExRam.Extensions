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

            return AsyncEnumerableExtensions
                .Create(() =>
                {
                    var e = enumerable.GetEnumerator();

                    return AsyncEnumerableExtensions.Create(
                        ct => gateTaskFunction(ct)
                            .Then(() => e.MoveNext(ct)),
                        () => e.Current,
                        e.Dispose);
                });
        }
    }
}