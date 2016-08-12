// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TSource> Defer<TSource>(Func<CancellationToken, Task<IAsyncEnumerable<TSource>>> asyncFactory)
        {
            if (asyncFactory == null)
                throw new ArgumentNullException(nameof(asyncFactory));

            return AsyncEnumerableExtensions.Create(
                () =>
                {
                    var baseEnumerator = default(IAsyncEnumerator<TSource>);

                    return AsyncEnumerableExtensions.Create(
                        async ct =>
                        {
                            if (baseEnumerator == null)
                                baseEnumerator = (await asyncFactory(ct).ConfigureAwait(false)).GetEnumerator();

                            return await baseEnumerator.MoveNext(ct).ConfigureAwait(false);
                        },
                        () =>
                        {
                            if (baseEnumerator == null)
                                throw new InvalidOperationException();

                            return baseEnumerator.Current;
                        },
                        () => baseEnumerator?.Dispose());
                });
        }
    }
}
