// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using Monad;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> SelectValueWhereAvailable<T>(this IAsyncEnumerable<OptionStrict<T>> enumerable)
        {
            return enumerable
                .Where(x => x.HasValue)
                .Select(x => x.Value);
        }
    }
}
