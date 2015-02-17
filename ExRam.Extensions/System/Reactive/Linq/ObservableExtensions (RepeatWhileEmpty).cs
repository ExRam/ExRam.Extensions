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
        public static IObservable<T> RepeatWhileEmpty<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return source.RepeatWhileEmpty(null);
        }

        public static IObservable<T> RepeatWhileEmpty<T>(this IObservable<T> source, int repeatCount)
        {
            Contract.Requires(source != null);

            return source.RepeatWhileEmpty((int?)repeatCount);
        }

        private static IObservable<T> RepeatWhileEmpty<T>(this IObservable<T> source, int? repeatCount)
        {
            Contract.Requires(source != null);
            Contract.Requires(!repeatCount.HasValue || repeatCount.Value >= 0);

            if ((repeatCount.HasValue) && (repeatCount.Value == 0))
                return Observable.Empty<T>();

            return source
                .Concat(maybe => !maybe.HasValue ? source.RepeatWhileEmpty(repeatCount.HasValue ? (int?)(repeatCount.Value - 1) : null) : Observable.Empty<T>());
        }
    }
}
