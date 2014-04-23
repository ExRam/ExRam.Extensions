// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
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
                        lock (syncRoot)
                        {
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
                            lock (syncRoot)
                            {
                                tcs.TrySetCanceled();
                            }
                        })));
            });
        }
    }
}
