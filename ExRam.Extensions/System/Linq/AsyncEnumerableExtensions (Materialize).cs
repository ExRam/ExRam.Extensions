// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<Notification<TSource>> Materialize<TSource>(this IAsyncEnumerable<TSource> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var completed = false;
                    var e = enumerable.GetEnumerator();
                    var current = default(Notification<TSource>);

                    return AsyncEnumerable.CreateEnumerator(
                        ct => e
                            .MoveNext(ct)
                            .ContinueWith(task =>
                            {
                                if (completed)
                                    return false;

                                if (task.IsFaulted)
                                {
                                    completed = true;
                                    current = Notification.CreateOnError<TSource>(task.Exception.InnerException);
                                }
                                else if (task.Result)
                                    current = Notification.CreateOnNext(e.Current);
                                else
                                {
                                    completed = true;
                                    current = Notification.CreateOnCompleted<TSource>();
                                }

                                return true;
                            }, TaskContinuationOptions.NotOnCanceled),
                        () => current,
                        e.Dispose);
                });
        }
    }
}
