// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<TAccumulate> ScanWithLeadingSeed<TSource, TAccumulate>(this IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
        {
            Contract.Requires(source != null);
            Contract.Requires(accumulator != null);

            return Observable.Return(seed)
                .Concat(source.Scan(seed, accumulator));
        }
    }
}
