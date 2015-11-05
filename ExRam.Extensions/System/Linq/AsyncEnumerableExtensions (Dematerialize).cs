// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Threading.Tasks;
using Monad;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<TSource> Dematerialize<TSource>(this IAsyncEnumerable<Notification<TSource>> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerableExtensions.Create(
                () =>
                {
                    var e = enumerable.GetEnumerator();
                    var current = default(TSource);

                    return AsyncEnumerableExtensions.Create(
                        ct =>
                        {
                            return e
                                .MoveNext(ct)
                                .ContinueWith(task =>
                                {
                                    if (task.IsFaulted)
                                        throw task.Exception.InnerException;

                                    if (task.Result)
                                    {
                                        if (e.Current.HasValue)
                                        {
                                            current = e.Current.Value;
                                            return true;
                                        }

                                        if (e.Current.Exception != null)
                                            throw e.Current.Exception;
                                    }
                                    
                                    return false;
                                }, TaskContinuationOptions.NotOnCanceled);
                        },
                        () => current,
                        e.Dispose);
                });
        }
    }
}
