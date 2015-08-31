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
        public static IAsyncEnumerable<TSource> Dematerialize<TSource>(this IAsyncEnumerable<Notification<TSource>> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable
                .Using(
                    enumerable.GetEnumerator,
                    e => AsyncEnumerable
                        .Repeat(Unit.Default)
                        .SelectMany((_, ct) => e.MoveNextAsMaybe(ct))
                        .Select(maybeNotification =>
                        {
                            if (maybeNotification.HasValue)
                            {
                                var notification = maybeNotification.Value;

                                switch (notification.Kind)
                                {
                                    case (NotificationKind.OnNext):
                                        return Maybe.Create(notification.Value);
                                    case (NotificationKind.OnError):
                                        throw notification.Exception;
                                }
                            }

                            return Maybe<TSource>.Null;
                        }))
                .TakeWhile(x => x.HasValue)
                .Select(x => x.Value);;
        }
    }
}
