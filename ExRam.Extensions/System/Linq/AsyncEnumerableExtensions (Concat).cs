// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> source, Func<Maybe<T>, IAsyncEnumerable<T>> continuationSelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(continuationSelector != null);

            return AsyncEnumerable2.Create(() =>
            {
                var lastValue = Maybe<T>.Null;
                IAsyncEnumerator<T> secondEnumerator = null;
                var firstEnumerator = source.GetEnumerator();
                var enumeratorDisposables = new SerialDisposable();

                if (firstEnumerator == null)
                    throw new InvalidOperationException();

                enumeratorDisposables.Disposable = firstEnumerator;

                return AsyncEnumeratorEx.Create(async (ct) =>
                {
                    while (true)
                    {
                        if (firstEnumerator != null)
                        {
                            var hasValue = await firstEnumerator.MoveNext(ct);

                            if (hasValue)
                                return lastValue = firstEnumerator.Current;

                            firstEnumerator = null;

                            var nextEnumerable = continuationSelector(lastValue);

                            if (nextEnumerable == null)
                                throw new InvalidOperationException();

                            secondEnumerator = nextEnumerable.GetEnumerator();

                            if (secondEnumerator == null)
                                throw new InvalidOperationException();

                            enumeratorDisposables.Disposable = secondEnumerator;
                        }
                        else if (secondEnumerator != null)
                        {
                            var hasValue = await secondEnumerator.MoveNext(ct);

                            if (hasValue)
                                return secondEnumerator.Current;

                            secondEnumerator = null;
                            enumeratorDisposables.Dispose();

                            return Maybe<T>.Null;
                        }
                        else
                            return Maybe<T>.Null;
                    }
                }, enumeratorDisposables.Dispose);
            });
        }
    }
}
