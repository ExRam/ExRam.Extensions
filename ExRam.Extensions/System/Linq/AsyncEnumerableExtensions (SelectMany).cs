// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, Task<TResult>> selector)
        {
            return enumerable
                .SelectMany((x, ct) => selector(x)); 
        }

        public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, Task<TResult>> selector)
        {
            return AsyncEnumerable
                .Using(
                    () => new CancellationDisposable(),
                    cts => enumerable.SelectMany(x => selector(x, cts.Token)
                        .ToAsyncEnumerable()));
        }

        public static IAsyncEnumerable<Unit> SelectMany<TSource>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, Task> selector)
        {
            return enumerable
                .SelectMany((x, ct) => selector(x));
        }

        public static IAsyncEnumerable<Unit> SelectMany<TSource>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, Task> selector)
        {
            return AsyncEnumerable
                .Using(
                    () => new CancellationDisposable(),
                    cts => enumerable.SelectMany(x => selector(x, cts.Token)
                        .ToAsyncEnumerable()));
        }
    }
}
