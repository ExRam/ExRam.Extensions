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
                    .Subscribe((notification) =>
                    {
                        TaskCompletionSource<Maybe<T>> localTcs;

                        lock (syncRoot)
                        {
                            if ((tcs.Task.IsFaulted) || (tcs.Task.IsCanceled))
                                return;

                            if (tcs.Task.IsCompleted)
                                tcs = new TaskCompletionSource<Maybe<T>>();

                            localTcs = tcs;
                        }

                        if (notification.HasValue)
                            localTcs.SetResult(notification.Value);
                        else if (notification.Exception != null)
                            localTcs.SetException(notification.Exception);
                        else
                            localTcs.SetResult(Maybe<T>.Null);
                    });

                return AsyncEnumeratorEx.Create(
                    (ct) =>
                    {
                        lock (syncRoot)
                        {
                            return tcs.Task.WithCancellation(ct);
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
