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

            var refCountObservable = source
                .RefCount();

            var innerObservable = Observable
                .Create<T>(
                    obs =>
                    {
                        var subscription = refCountObservable.Subscribe(obs);

                        return Disposable.Create(() =>
                        {
                            scheduler.Schedule(Unit.Default, delay, (self, state) =>
                            {
                                subscription.Dispose();
                            });
                        });
                    });

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
