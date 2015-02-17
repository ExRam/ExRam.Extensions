// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Threading;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> SubscribeConcurrentlyAtMost<T>(this IObservable<T> source, int count, IObservable<T> continuation)
        {
            Contract.Requires(source != null);
            Contract.Requires(count >= 0);
            Contract.Requires(continuation != null);

            var subscriptionCount = 0;

            return Observable.Create<T>(observer =>
            {
                while (true)
                {
                    var localSubscriptionCount = subscriptionCount;
                    if (localSubscriptionCount >= count)
                        return continuation.Subscribe(observer);

                    if (Interlocked.CompareExchange(ref subscriptionCount, localSubscriptionCount + 1, localSubscriptionCount) == localSubscriptionCount)
                    {
                        var subscription = source.Subscribe(observer);

                        return Disposables.Disposable.Create(() =>
                        {
                            Interlocked.Decrement(ref subscriptionCount);
                            subscription.Dispose();
                        });
                    }
                }
            });
        }
    }
}
