// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using LanguageExt;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> Concat<T>(this IObservable<T> source, Func<Option<T>, IObservable<T>> continuationSelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(continuationSelector != null);

            return Observable.Create<T>(obs =>
            {
                var lastValue = Option<T>.None;
                var subscription = new SerialDisposable();
                var firstSubscription = new SingleAssignmentDisposable();

                subscription.Disposable = firstSubscription;

                firstSubscription.Disposable = source.Subscribe(
                    value => 
                    {
                        lastValue = value;
                        obs.OnNext(value);
                    },
                    ex =>
                    {
                        subscription.Dispose();
                        obs.OnError(ex);
                    },
                    () =>
                    {
                        subscription.Disposable.Dispose();
                        subscription.Disposable = continuationSelector(lastValue).Subscribe(obs);
                    });

                return subscription;
            });
        }
    }
}
