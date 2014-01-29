using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static partial class AsyncEnumeratorEx
    {
        public static async Task<Maybe<T>> MoveNextAsMaybe<T>(this IAsyncEnumerator<T> enumerator, CancellationToken ct)
        {
            if (await enumerator.MoveNext(ct))
                return enumerator.Current;

            return Maybe<T>.Null;
        }
    }
}
