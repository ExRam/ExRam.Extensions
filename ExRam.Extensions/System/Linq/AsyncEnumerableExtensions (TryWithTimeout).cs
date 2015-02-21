// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<Maybe<T>> TryWithTimeout<T>(this IAsyncEnumerable<T> enumerable, TimeSpan timeout)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable
                .Using(
                    enumerable.GetEnumerator,
                    e => AsyncEnumerable
                        .Repeat(Unit.Default)
                        .SelectMany((_, ct) => e.MoveNextAsMaybe(ct).TryWithTimeout(timeout))
                        .TakeWhile(maybe => maybe.HasValue));
        }
    }
}