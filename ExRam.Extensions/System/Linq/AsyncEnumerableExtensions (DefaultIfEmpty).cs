// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> DefaultIfEmpty<T>(this IAsyncEnumerable<T> source, IAsyncEnumerable<T> defaultObservable)
        {
            Contract.Requires(source != null);
            Contract.Requires(defaultObservable != null);

            return source.Concat(maybe => !maybe.IsSome ? defaultObservable : AsyncEnumerable.Empty<T>());
        }
    }
}
