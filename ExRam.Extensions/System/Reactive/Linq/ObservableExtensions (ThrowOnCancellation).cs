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
        public static IObservable<T> ThrowOnCancellation<T>(this IObservable<T> source, CancellationToken ct)
        {
            Contract.Requires(source != null);

            return source.TakeUntil(ct)
                .Concat(Observable.If(
                    () => ct.IsCancellationRequested,
                    Observable.Throw<T>(new OperationCanceledException())));
        }
    }
}
