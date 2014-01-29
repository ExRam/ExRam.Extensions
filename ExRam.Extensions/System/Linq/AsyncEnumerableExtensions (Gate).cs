// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> Gate<T>(this IAsyncEnumerable<T> enumerable, Func<Task> gateTaskFunction)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(gateTaskFunction != null);

            return AsyncEnumerable2.Create(() =>
            {
                var enumerator = enumerable.GetEnumerator();

                return AsyncEnumeratorEx.Create(
                    async (ct) =>
                        {
                            await gateTaskFunction().ConfigureAwait(false);
                            return await enumerator.MoveNextAsMaybe(ct);
                        },
                    enumerator.Dispose);
            });
        }

        public static IAsyncEnumerable<T> Gate<T>(this IAsyncEnumerable<T> enumerable, Func<Maybe<T>, Task> gateTaskFunction)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(gateTaskFunction != null);

            return AsyncEnumerable2.Create(() =>
            {
                var maybeValue = Maybe<T>.Null;
                var enumerator = enumerable.GetEnumerator();

                return AsyncEnumeratorEx.Create(
                    async (ct) =>
                    {
                        await gateTaskFunction(maybeValue).ConfigureAwait(false);
                        return maybeValue = await enumerator.MoveNextAsMaybe(ct);
                    },
                    enumerator.Dispose);
            });
        }
    }
}