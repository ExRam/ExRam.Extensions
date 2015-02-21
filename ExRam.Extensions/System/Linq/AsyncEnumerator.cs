// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class AsyncEnumeratorEx
    {
        public static async Task<Maybe<T>> MoveNextAsMaybe<T>(this IAsyncEnumerator<T> enumerator, CancellationToken ct)
        {
            if (await enumerator.MoveNext(ct))
                return enumerator.Current;

            return Maybe<T>.Null;
        }
    }
}
