// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IAsyncEnumerable<T> Current<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            var replayedSource = source.Replay(1);

            return AsyncEnumerable.Using(
                replayedSource.Connect,
                connection => AsyncEnumerable
                    .Repeat(Unit.Default)
                    .SelectAsync(x => replayedSource.FirstAsync().ToTask()));
        }
    }
}
