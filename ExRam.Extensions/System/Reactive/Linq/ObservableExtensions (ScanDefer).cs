// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<TAccumulate> ScanDefer<TSource, TAccumulate>(this IObservable<TSource> source, Func<TAccumulate> seedFunction, Func<TAccumulate, TSource, TAccumulate> accumulator)
        {
            Contract.Requires(source != null);
            Contract.Requires(seedFunction != null);
            Contract.Requires(accumulator != null);

            return source
                .Scan(
                    seedFunction,
                    (f, value) =>
                    {
                        var ret = accumulator(f(), value);
                        return () => ret;       //DO NOT INLINE!
                    })
                .Select(f => f());
        }
    }
}
