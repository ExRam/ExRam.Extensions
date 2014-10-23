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
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, Task<bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);

            return source
                .SelectMany(x => predicate(x)
                    .ToObservable()
                    .Select(b => b ? x : Maybe<T>.Null))
                .Where(x => x.HasValue)
                .Select(x => x.Value);
        }
    }
}
