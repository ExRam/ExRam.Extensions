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
        public static IAsyncEnumerable<T> Gate<T>(this IAsyncEnumerable<T> enumerable, Func<CancellationToken, Task> gateTaskFunction)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(gateTaskFunction != null);

            return AsyncEnumerable
                .CreateEnumerable(() =>
                {
                    var e = enumerable.GetEnumerator();

                    return AsyncEnumerable.CreateEnumerator(
                        ct => gateTaskFunction(ct)
                            .Then(() => e.MoveNext(ct)),
                        () => e.Current,
                        e.Dispose);
                });
        }
    }
}