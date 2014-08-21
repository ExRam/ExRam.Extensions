// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> DefaultIfEmpty<T>(this IObservable<T> source, IObservable<T> defaultObservable)
        {
            Contract.Requires(source != null);
            Contract.Requires(defaultObservable != null);

            return source.Concat((maybe) => !maybe.HasValue ? defaultObservable : Observable.Empty<T>());
        }
    }
}
