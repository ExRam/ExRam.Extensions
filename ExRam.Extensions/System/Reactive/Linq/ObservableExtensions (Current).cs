// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IAsyncEnumerable<T> Current<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return AsyncEnumerable2.Create(() =>
            {
                var syncRoot = new object();
                var tcs = new TaskCompletionSource<Maybe<T>>();

                var subscription = source
                    .Materialize()
                    .Subscribe(notification =>
                    {
                        lock (syncRoot)
                        {
                            if ((tcs.Task.IsFaulted) || (tcs.Task.IsCanceled))
                                return;

                            if (tcs.Task.IsCompleted)
                                tcs = new TaskCompletionSource<Maybe<T>>();

                            if (notification.HasValue)
                                tcs.SetResult(notification.Value);
                            else if (notification.Exception != null)
                                tcs.SetException(notification.Exception);
                            else
                                tcs.SetResult(Maybe<T>.Null);
                        }
                    });

                return AsyncEnumeratorEx.Create(
                    ct =>
                    {
                        lock (syncRoot)
                        {
                            if ((!tcs.Task.IsCompleted) && (!tcs.Task.IsCanceled))
                                return tcs.Task.WithCancellation(ct);

                            return tcs.Task;
                        }
                    },
                    new CompositeDisposable(
                        subscription,
                        Disposable.Create(() =>
                        {
                            TaskCompletionSource<Maybe<T>> localTcs;

                            lock (syncRoot)
                            {
                                localTcs = tcs;
                            }

                            localTcs.TrySetCanceled();
                        })));
            });
        }
    }
}
