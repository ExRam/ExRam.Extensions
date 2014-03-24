// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Threading;

namespace System.Reactive.Subjects
{
    public static partial class ConnectableObservableExtensions
    {
        public static IConnectableObservable<T> ConnectTotallyAtMost<T>(this IConnectableObservable<T> source, int count, IObservable<T> continuation)
        {
            Contract.Requires(source != null);
            Contract.Requires(count >= 0);
            Contract.Requires(continuation != null);

            var connectionCount = 0;

            return ConnectableObservable.Create<T>(() =>
            {
                while (true)
                {
                    var localConnectionCount = connectionCount;
                    if (localConnectionCount >= count)
                        return Disposables.Disposable.Empty;

                    if (Interlocked.CompareExchange(ref connectionCount, localConnectionCount + 1, localConnectionCount) == localConnectionCount)
                        return source.Connect();
                }
            }, source.Subscribe);
        }
    }
}
