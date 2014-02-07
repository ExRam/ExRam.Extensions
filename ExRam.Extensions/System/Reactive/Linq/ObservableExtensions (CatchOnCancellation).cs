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
        public static IObservable<T> CatchOnCancellation<T>(this IObservable<T> source, CancellationToken ct, IObservable<T> cancellationContinuation)
        {
            Contract.Requires(source != null);

            return source.TakeUntil(ct)
                .Concat(Observable.If(
                    () => ct.IsCancellationRequested,
                    cancellationContinuation));
        }
    }
}
