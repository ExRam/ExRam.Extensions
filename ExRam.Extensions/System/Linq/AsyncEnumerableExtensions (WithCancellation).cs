// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using System.Threading;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> WithCancellation<T>(Func<CancellationToken, IAsyncEnumerable<T>> enumerableFactory)
        {
            Contract.Requires(enumerableFactory != null);

            return AsyncEnumerable
                .Using(
                    () => new CancellationDisposable(),
                    cts => enumerableFactory(cts.Token));
        }
    }
}
