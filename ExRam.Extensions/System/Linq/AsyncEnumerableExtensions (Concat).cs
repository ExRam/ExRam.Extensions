// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> source, Func<Maybe<T>, IAsyncEnumerable<T>> continuationSelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(continuationSelector != null);

            return source
                .Materialize()
                .Scan(
                    new
                    {
                        Previous = (Notification<T>)null,
                        Current = (Notification<T>)null
                    },
                    (previous, current) => new
                    {
                        Previous = previous.Current,
                        Current = current
                    })
                .SelectMany(tuple =>
                {
                    if (tuple.Current.HasValue)
                        return AsyncEnumerable.Return(tuple.Current.Value);

                    if (tuple.Current.Exception != null)
                        return AsyncEnumerable.Throw<T>(tuple.Current.Exception);

                    return tuple.Previous != null
                        ? continuationSelector(tuple.Previous.Value)
                        : continuationSelector(Maybe<T>.Null);
                });
        }
    }
}
