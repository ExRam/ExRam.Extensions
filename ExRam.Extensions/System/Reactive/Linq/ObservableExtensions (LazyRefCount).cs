// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> LazyRefCount<T>(this IConnectableObservable<T> source, TimeSpan delay)
        {
            Contract.Requires(source != null);

            return source.LazyRefCount(delay, Scheduler.Default);
        }

        public static IObservable<T> LazyRefCount<T>(this IConnectableObservable<T> source, TimeSpan delay, IScheduler scheduler)
        {
            Contract.Requires(source != null);

            var syncRoot = new object();
            var serial = new SerialDisposable();
            IDisposable currentConnection = null;

            var innerObservable = ConnectableObservable
                .Create<T>(
                    () =>
                    {
                        lock (syncRoot)
                        {
                            if (currentConnection == null)
                                currentConnection = source.Connect();

                            serial.Disposable = new SingleAssignmentDisposable();
                        }

                        return Disposable
                            .Create(() =>
                            {
                                lock (syncRoot)
                                {
                                    var cancelable = (SingleAssignmentDisposable)serial.Disposable;

                                    cancelable.Disposable = scheduler.Schedule(cancelable, delay, (self, state) =>
                                    {
                                        lock (syncRoot)
                                        {
                                            if (object.ReferenceEquals(serial.Disposable, state))
                                            {
                                                currentConnection.Dispose();
                                                currentConnection = null;
                                            }
                                        }

                                        return Disposable.Empty;
                                    });
                                }
                            });
                    },
                    source.Subscribe)
                .RefCount();

            return Observable.Create<T>(
                outerObserver =>
                {
                    var anonymousObserver = new AnonymousObserver<T>(
                        outerObserver.OnNext,
                        outerObserver.OnError,
                        outerObserver.OnCompleted);

                    return new CompositeDisposable(
                        anonymousObserver,
                        innerObservable
                            .Subscribe(anonymousObserver));
                });
        }
    }
}
