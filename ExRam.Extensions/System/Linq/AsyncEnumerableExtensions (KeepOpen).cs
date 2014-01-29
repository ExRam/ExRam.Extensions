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
        public static IAsyncEnumerable<T> KeepOpen<T>(this IAsyncEnumerable<T> enumerable)
        {
            Contract.Requires(enumerable != null);

            return enumerable.Concat(AsyncEnumerable.Never<T>());
        }
    }
}
