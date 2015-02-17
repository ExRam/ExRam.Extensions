// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<Notification<TSource>> Materialize<TSource>(this IAsyncEnumerable<TSource> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable2.Create(() =>
            {
                var syncRoot = new object();
                var completedOrError = false;
                var e = enumerable.GetEnumerator();

                return AsyncEnumeratorEx.Create(
                    async ct =>
                    {
                        lock (syncRoot)
                        {
                            if (completedOrError)
                                return Maybe<Notification<TSource>>.Null;
                        }

                        try
                        {
                            if (await e.MoveNext(ct))
                                return Notification.CreateOnNext(e.Current);

                            lock (syncRoot)
                            {
                                completedOrError = true;
                            }

                            return Notification.CreateOnCompleted<TSource>();
                        }
                        catch (Exception ex)
                        {
                            lock (syncRoot)
                            {
                                completedOrError = true;
                            }

                            return Notification.CreateOnError<TSource>(ex);
                        }
                    },
                    e.Dispose);
            });
        }
    }
}
