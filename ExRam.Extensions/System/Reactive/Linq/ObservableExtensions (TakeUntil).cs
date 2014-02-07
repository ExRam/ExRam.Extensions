// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Threading;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> TakeUntil<T>(this IObservable<T> source, CancellationToken ct)
        {
            return source.TakeUntil(ct.ToObservable());
        }
    }
}
