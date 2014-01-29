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
        public static IObservable<T> KeepOpen<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return source.Concat(Observable.Never<T>());
        }
    }
}
