// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> KeepOpen<T>(this IAsyncEnumerable<T> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerableExtensions.Create(() =>
            {
                var e = enumerable.GetEnumerator();

                return AsyncEnumerableExtensions.Create(
                    (ct, tcs) =>
                    {
                        e.MoveNext(ct)
                            .ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                    tcs.TrySetException(task.Exception.InnerException);

                                if (task.Result)
                                    tcs.TrySetResult(true);
                            }, TaskContinuationOptions.NotOnCanceled);

                        return tcs.Task;
                    },
                    () => e.Current,
                    e.Dispose);
            });
        }
    }
}
