// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
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
        public static IAsyncEnumerable<T> Append<T>(this IAsyncEnumerable<T> enumerable, T value)
        {
            Contract.Requires(enumerable != null);

            return enumerable.Concat(AsyncEnumerable.Return(value));
        }
    }
}
