// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<Unit> SelectMany<TSource>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task> taskSelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(taskSelector != null);

            return source
                .SelectMany((x, ct) => taskSelector(x, ct).AsUnitTask());
        }
    }
}
