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
        public static IObservable<T> SubscribeAtMost<T>(this IObservable<T> source, int count)
        {
            Contract.Requires(source != null);
            Contract.Requires(count >= 0);

            var subscriptionCount = 0;

            return Observable.Create<T>((observer) =>
            {
                while (true)
                {
                    var localSubscriptionCount = subscriptionCount;
                    if (localSubscriptionCount >= count)
                        return Disposables.Disposable.Empty;

                    if (Interlocked.CompareExchange(ref subscriptionCount, localSubscriptionCount + 1, localSubscriptionCount) == localSubscriptionCount)
                        return source.Subscribe(observer);
                }
            });
        }
    }
}
