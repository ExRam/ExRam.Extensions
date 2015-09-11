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
                        var schedulerSubscription = new SingleAssignmentDisposable();

                        lock (syncRoot)
                        {
                            if (currentConnection == null)
                                currentConnection = source.Connect();

                            serial.Disposable = schedulerSubscription;
                        }

                        return Disposable
                            .Create(() =>
                            {
                                schedulerSubscription.Disposable = scheduler.Schedule(schedulerSubscription, delay, (self, state) =>
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
