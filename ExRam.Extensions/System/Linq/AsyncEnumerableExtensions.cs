// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Unit = System.Reactive.Unit;

namespace System.Linq
{
    public static class AsyncEnumerableExtensions
    {
        private sealed class JoinStream : Stream
        {
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();
            private readonly IAsyncEnumerator<ArraySegment<byte>> _arraySegmentEnumerator;

            private ArraySegment<byte>? _currentInputSegment;

            public JoinStream(IAsyncEnumerable<ArraySegment<byte>> segmentEnumerable)
            {
                _arraySegmentEnumerator = segmentEnumerable.GetAsyncEnumerator(_cts.Token);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return ReadAsync(buffer, offset, count, CancellationToken.None).Result;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (count == 0)
                    return 0;

                using (cancellationToken.CanBeCanceled ? cancellationToken.Register(() => _cts.Cancel()) : Disposable.Empty)
                {
                    ArraySegment<byte> currentInputSegment;
                    var currentNullableInputSegment = _currentInputSegment;

                    if (currentNullableInputSegment == null)
                    {
                        try
                        {
                            if (await _arraySegmentEnumerator.MoveNextAsync().ConfigureAwait(false))
                                currentInputSegment = _arraySegmentEnumerator.Current;
                            else
                                return 0;
                        }
                        catch (AggregateException ex)
                        {
                            throw ex.GetBaseException();
                        }
                    }
                    else
                        currentInputSegment = currentNullableInputSegment.Value;

                    var minToRead = Math.Min(currentInputSegment.Count, count);
                    Buffer.BlockCopy(currentInputSegment.Array, currentInputSegment.Offset, buffer, offset, minToRead);

                    currentInputSegment = new ArraySegment<byte>(currentInputSegment.Array, currentInputSegment.Offset + minToRead, currentInputSegment.Count - minToRead);
                    _currentInputSegment = currentInputSegment.Count > 0 ? (ArraySegment<byte>?)currentInputSegment : null;

                    return minToRead;
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _cts.Cancel();
                    _arraySegmentEnumerator.DisposeAsync();
                }

                base.Dispose(disposing);
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
        }

        public static IAsyncEnumerable<TSource> Concat<TSource>(this IAsyncEnumerable<TSource> source, Func<Option<TSource>, IAsyncEnumerable<TSource>> continuationSelector)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<TSource> Core(CancellationToken cancellationToken)
            {
                var last = default(Option<TSource>);

                await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    last = item;
                    yield return item;
                }

                await foreach (var item in continuationSelector(last).WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return item;
                }
            }
        }

        public static IAsyncEnumerable<TSource> DefaultIfEmpty<TSource>(this IAsyncEnumerable<TSource> source, IAsyncEnumerable<TSource> defaultObservable)
        {
            return source.Concat(maybe => !maybe.IsSome ? defaultObservable : AsyncEnumerable.Empty<TSource>());
        }

        public static IAsyncEnumerable<TSource> Dematerialize<TSource>(this IAsyncEnumerable<Notification<TSource>> enumerable)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<TSource> Core(CancellationToken cancellationToken)
            {
                await foreach (var notification in enumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    if (notification.HasValue)
                        yield return notification.Value;
                    else if (notification.Exception != null)
                        throw notification.Exception;
                    else
                        yield break;
                }
            }
        }

        public static IAsyncEnumerable<TSource> DelayedEmpty<TSource>(TimeSpan delay)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<TSource> Core(CancellationToken cancellationToken)
            {
                await Task.Delay(delay, cancellationToken);

                yield break;
            }
        }

        public static IAsyncEnumerable<TSource> Gate<TSource>(this IAsyncEnumerable<TSource> enumerable, Func<CancellationToken, Task> gateTaskFunction)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<TSource> Core(CancellationToken cancellationToken)
            {
                await gateTaskFunction(cancellationToken);

                await foreach (var item in enumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return item;
                    await gateTaskFunction(cancellationToken);
                }
            }
        }

        public static IAsyncEnumerable<TSource> KeepOpen<TSource>(this IAsyncEnumerable<TSource> enumerable)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<TSource> Core(CancellationToken cancellationToken)
            {
                await foreach (var item in enumerable.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    yield return item;
                }

                if (cancellationToken.CanBeCanceled)
                {
                    var tcs = new TaskCompletionSource<object>();

                    using (cancellationToken.Register(() => tcs.SetCanceled()))
                    {
                        await tcs.Task;
                    }
                }
                else
                {
                    await new TaskCompletionSource<object>().Task;
                }
            }
        }

        public static IAsyncEnumerable<Notification<TSource>> Materialize<TSource>(this IAsyncEnumerable<TSource> enumerable)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<Notification<TSource>> Core(CancellationToken cancellationToken)
            {
                await using (var enumerator = enumerable.GetAsyncEnumerator(cancellationToken))
                {
                    Notification<TSource> notification;

                    do
                    {

                        try
                        {
                            notification = await enumerator.MoveNextAsync()
                                ? Notification.CreateOnNext(enumerator.Current)
                                : Notification.CreateOnCompleted<TSource>();
                        }
                        catch (Exception ex)
                        {
                            notification = Notification.CreateOnError<TSource>(ex);
                        }

                        yield return notification;
                    } while (notification.Kind == NotificationKind.OnNext);
                }
            }
        }

        public static IAsyncEnumerable<TAccumulate> ScanAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulator)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<TAccumulate> Core(CancellationToken cancellationToken)
            {
                var acc = seed;

                await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    acc = await accumulator(acc, item, cancellationToken);

                    yield return acc;
                }
            }
        }

        public static IAsyncEnumerable<Unit> SelectAwaitWithCancellation<TSource>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, ValueTask> selector)
        {
            return enumerable
                .SelectAwaitWithCancellation(async (x, ct) =>
                {
                    await selector(x, ct);

                    return Unit.Default;
                });
        }

        public static Stream ToStream(this IAsyncEnumerable<ArraySegment<byte>> byteSegmentAsyncEnumerable) => new JoinStream(byteSegmentAsyncEnumerable);

        public static Task<Option<T>> HeadOrNone<T>(this IAsyncEnumerable<T> enumerable)
        {
            return enumerable.HeadOrNone(CancellationToken.None);
        }

        public static async Task<Option<TSource>> HeadOrNone<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken token)
        {
            await foreach (var item in source.WithCancellation(token).ConfigureAwait(false))
            {
                return item;
            }

            return default;
        }

        public static IAsyncEnumerable<TSource> TryWithTimeout<TSource>(this IAsyncEnumerable<TSource> enumerable, TimeSpan timeout)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<TSource> Core(CancellationToken cancellationToken)
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    await using (var enumerator = enumerable.GetAsyncEnumerator(cts.Token))
                    {
                        while (true)
                        {
                            var option = await enumerator
                                .MoveNextAsync()
                                .AsTask()
                                .TryWithTimeout(timeout)
                                .ConfigureAwait(false);

                            if (option.IfNone(false))
                                yield return enumerator.Current;
                            else
                            {
                                cts.Cancel();

                                break;
                            }
                        }
                    }
                }
            }
        }

        public static IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T> source)
            where T : class
        {
            return source.Where(t => !ReferenceEquals(t, default(T)));
        }

        public static IAsyncEnumerable<TTarget> SelectMany<TSource, TTarget>(this IAsyncEnumerable<TSource> source, Func<TSource, IEnumerable<TTarget>> selector)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<TTarget> Core(CancellationToken cancellationToken)
            {
                await foreach(var item in source.WithCancellation(cancellationToken))
                {
                    foreach(var subItem in selector(item))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        yield return subItem;
                    }
                }
            }
        }
    }
}
