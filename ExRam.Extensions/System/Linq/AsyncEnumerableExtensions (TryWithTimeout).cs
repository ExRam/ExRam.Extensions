// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> TryWithTimeout<T>(this IAsyncEnumerable<T> enumerable, TimeSpan timeout)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var e = enumerable.GetEnumerator();

                    return AsyncEnumerable.CreateEnumerator(
                        async ct =>
                        {
                            using (var cts = new CancellationDisposable())
                            {
                                var option = await e
                                    .MoveNext(CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token).Token)
                                    .TryWithTimeout(timeout)
                                    .ConfigureAwait(false);

                                return option.Exists(_ => _);
                            }
                        },
                        () => e.Current,
                        e.Dispose);
                });
        }
    }
}