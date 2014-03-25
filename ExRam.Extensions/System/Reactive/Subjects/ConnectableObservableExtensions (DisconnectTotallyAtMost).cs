// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace System.Reactive.Subjects
{
    public static partial class ConnectableObservableExtensions
    {
        public static IConnectableObservable<T> DisconnectTotallyAtMost<T>(this IConnectableObservable<T> source, int count)
        {
            Contract.Requires(source != null);
            Contract.Requires(count >= 0);

            var disconnectionCount = 0;

            return ConnectableObservable.Create<T>(() =>
            {
                var connection = source.Connect();

                return Disposable.Create(() =>
                {
                    while (true)
                    {
                        var localConnectionCount = disconnectionCount;
                        if (localConnectionCount >= count)
                            return;

                        if (Interlocked.CompareExchange(ref disconnectionCount, localConnectionCount + 1, localConnectionCount) == localConnectionCount)
                        {
                            connection.Dispose();
                            return;
                        }
                    }
                });
            }, source.Subscribe);
        }
    }
}
