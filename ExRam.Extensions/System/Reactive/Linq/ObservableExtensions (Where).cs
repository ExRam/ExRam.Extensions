// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, CancellationToken, Task<bool>> predicate)
        {
            Contract.Requires(source != null);
            Contract.Requires(predicate != null);

            return source
                .SelectMany(x => ObservableExtensions
                    .WithCancellation(
                        ct => predicate(x, ct)
                            .ToObservable()
                            .Where(b => b)
                            .Select(b => x)));
        }
    }
}
