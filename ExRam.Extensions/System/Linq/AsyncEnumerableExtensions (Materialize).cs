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

            return AsyncEnumerable
                .Using(
                    enumerable.GetEnumerator,
                    e =>
                    {
                        var completed = false;

                        return AsyncEnumerable
                            .Repeat(Unit.Default)
                            .TakeWhile(_ => !completed)
                            .SelectMany(async (_, ct) =>
                            {
                                try
                                {
                                    var maybe = await e.MoveNextAsMaybe(ct);

                                    return maybe.HasValue 
                                        ? Notification.CreateOnNext(maybe.Value) 
                                        : Notification.CreateOnCompleted<TSource>();
                                }
                                catch (AggregateException ex)
                                {
                                    return Notification.CreateOnError<TSource>(ex.GetBaseException());
                                }
                            })
                            .Do(notification => completed |= !notification.HasValue);
                    });
        }
    }
}
