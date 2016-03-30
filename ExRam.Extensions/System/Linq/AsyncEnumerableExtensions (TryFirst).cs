// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static Task<Option<T>> TryFirst<T>(this IAsyncEnumerable<T> enumerable)
        {
            return enumerable.TryFirst(CancellationToken.None);
        }

        public static async Task<Option<T>> TryFirst<T>(this IAsyncEnumerable<T> enumerable, CancellationToken token)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                if (await enumerator.MoveNext(token).ConfigureAwait(false))
                    return enumerator.Current;

                return Option<T>.None;
            }
        }
    }
}
