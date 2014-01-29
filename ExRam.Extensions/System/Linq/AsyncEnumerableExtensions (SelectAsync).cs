// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TResult> SelectAsync<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, Task<TResult>> selector)
        {
            return enumerable.SelectMany(x => selector(x).ToAsyncEnumerable());
        }
    }
}
