// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        private sealed class UnwrappingAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IAsyncEnumerator<T> _baseEnumerator;

            public UnwrappingAsyncEnumerator(IAsyncEnumerator<T> baseEnumerator)
            {
                Contract.Requires(baseEnumerator != null);

                this._baseEnumerator = baseEnumerator;
            }

            public T Current
            {
                get
                {
                    return this._baseEnumerator.Current;
                }
            }

            public async Task<bool> MoveNext(CancellationToken ct)
            {
                try
                {
                    return await this._baseEnumerator.MoveNext(ct);
                }
                catch (AggregateException ex)
                {
                    throw ex.GetBaseException();
                }
            }

            public void Dispose()
            {
                this._baseEnumerator.Dispose();
            }
        }

        public static IAsyncEnumerable<T> Unwrap<T>(this IAsyncEnumerable<T> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerableExtensions.Create(() => new UnwrappingAsyncEnumerator<T>(enumerable.GetEnumerator()));
        }
    }
}
