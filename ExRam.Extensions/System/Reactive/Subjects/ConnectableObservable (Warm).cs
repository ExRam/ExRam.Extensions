// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Reactive.Linq;
using System.Diagnostics.Contracts;

namespace System.Reactive.Subjects
{
    public static partial class ConnectableObservable
    {
        public static IObservable<T> Warm<T>(this IConnectableObservable<T> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IObservable<T>>() != null);

            var isConnected = false;
            var syncRoot = new object();

            return Observable.Create<T>(observer =>
            {
                var subscription = source.Subscribe(observer);

                lock (syncRoot)
                {
                    if (!isConnected)
                    {
                        isConnected = true;

                        source.Connect();
                    }
                }

                return subscription;
            });
        }
    }
}
