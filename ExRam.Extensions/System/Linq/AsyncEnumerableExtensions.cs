// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Unit = System.Reactive.Unit;

namespace System.Linq
{
    public static class AsyncEnumerableExtensions
    {
        internal class Yielder<T> : IYielder<T>, IAwaitable, IAwaiter, IAsyncEnumerator<T>
        {
            private struct MoveNextAwaitable : ICriticalNotifyCompletion
            {
                private readonly Yielder<T> _yielder;

                public MoveNextAwaitable(Yielder<T> yielder)
                {
                    this._yielder = yielder;
                }

                public MoveNextAwaitable GetAwaiter()
                {
                    return this;
                }

                // ReSharper disable once MemberCanBeMadeStatic.Local
                public void GetResult()
                {

                }

                public void OnCompleted(Action continuation)
                {
                    lock (this._yielder)
                    {
                        if (this._yielder._state != YielderState.InFlight)
                            continuation();
                        else
                            this._yielder._moveNextContinuation = continuation;
                    }
                }

                public void UnsafeOnCompleted(Action continuation)
                {
                    this.OnCompleted(continuation);
                }

                public bool IsCompleted
                {
                    get
                    {
                        lock (this._yielder)
                        {
                            return this._yielder._state != YielderState.InFlight;
                        }
                    }
                }
            }

            private enum YielderState : byte
            {
                NotStarted,
                Idle,
                InFlight,
                HasValue,
                Stopped
            }

            private readonly MoveNextAwaitable _moveNextAwaitable;
            private readonly Func<CancellationToken, Yielder<T>, Task> _createAction;
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            private Exception _exception;
            private Action _yieldContinuation;
            private Action _moveNextContinuation;
            private YielderState _state = YielderState.NotStarted;

            public Yielder(Func<CancellationToken, Yielder<T>, Task> create)
            {
                this._createAction = create;
                this._moveNextAwaitable = new MoveNextAwaitable(this);
            }

            public IAwaiter GetAwaiter()
            {
                return this;
            }

            public void GetResult()
            {
                lock (this)
                {
                    if (this._state == YielderState.Stopped)
                        throw new OperationCanceledException();
                }
            }

            [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                this.OnCompleted(continuation);
            }

            public void OnCompleted(Action continuation)
            {
                lock (this)
                {
                    if (this._state == YielderState.InFlight || this._state == YielderState.Stopped)
                        continuation();
                    else
                        this._yieldContinuation = continuation;
                }
            }

            public IAwaitable Return(T value)
            {
                return this.Yield(YielderState.HasValue, value);
            }

            public IAwaitable Break()
            {
                return this.Yield(YielderState.Stopped);
            }

            private IAwaitable Yield(YielderState newState, T current = default(T))
            {
                lock (this)
                {
                    if (this._state == YielderState.InFlight)
                    {
                        this._state = newState;
                        this.Current = current;
                        this._yieldContinuation = null;
                        this._moveNextContinuation?.Invoke();
                    }
                }

                return this;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                using (cancellationToken.Register(this._cts.Cancel))
                {
                    lock (this)
                    {
                        if (this._state == YielderState.InFlight || this._state == YielderState.HasValue)
                            throw new InvalidOperationException();

                        if (this._state == YielderState.Stopped)
                            return false;

                        this._moveNextContinuation = null;

                        if (this._state == YielderState.Idle)
                        {
                            this._state = YielderState.InFlight;
                            this._yieldContinuation?.Invoke();
                        }
                        else
                        {
                            this._state = YielderState.InFlight;

                            #pragma warning disable 4014
                            this._createAction(this._cts.Token, this)
                                // ReSharper disable once MethodSupportsCancellation
                                .ContinueWith(
                                    t =>
                                    {
                                        lock (this)
                                        {
                                            if (t.IsFaulted)
                                                this._exception = t.Exception;

                                            if (!t.IsCanceled)
                                                this.Break();
                                        }
                                    },
                                    TaskContinuationOptions.ExecuteSynchronously);
                            #pragma warning restore 4014
                        }
                    }

                    await this._moveNextAwaitable;

                    lock (this)
                    {
                        if (this._exception != null)
                            throw this._exception;

                        var ret = this._state == YielderState.HasValue;
                        if (ret)
                            this._state = YielderState.Idle;

                        return ret;
                    }
                }
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
                lock (this)
                {
                    this._cts.Cancel();
                    this._state = YielderState.Stopped;

                    this._yieldContinuation?.Invoke();
                }
            }

            public T Current
            {
                get;
                private set;
            }

            public bool IsCompleted
            {
                get
                {
                    lock (this)
                    {
                        return this._state == YielderState.InFlight;
                    }
                }
            }
        }
        
        public static IAsyncEnumerable<T> Append<T>(this IAsyncEnumerable<T> enumerable, T value)
        {
            Contract.Requires(enumerable != null);

            return enumerable.Concat(AsyncEnumerable.Return(value));
        }

        public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> source, Func<Option<T>, IAsyncEnumerable<T>> continuationSelector)
        {
            Contract.Requires(source != null);
            Contract.Requires(continuationSelector != null);

            return source
                .Materialize()
                .Scan(
                    (Previous: (Notification<T>)null, Current: (Notification<T>)null),
                    (tuple, current) => (Previous: tuple.Current, Current: current))
                .SelectMany(tuple =>
                {
                    if (tuple.Current.HasValue)
                        return AsyncEnumerable.Return(tuple.Current.Value);

                    if (tuple.Current.Exception != null)
                        return AsyncEnumerable.Throw<T>(tuple.Current.Exception);

                    return tuple.Previous != null
                        ? continuationSelector(tuple.Previous.Value)
                        : continuationSelector(Option<T>.None);
                });
        }

        public static IAsyncEnumerable<T> Create<T>(Func<CancellationToken, IYielder<T>, Task> create)
        {
            if (create == null)
                throw new ArgumentNullException(nameof(create));

            //Wrapping in CreateEnumerable muss sein, da Yielder nur IAsyncEnumerator implementiert und immer neu erzeugt werden muss.
            return AsyncEnumerable.CreateEnumerable(() => new Yielder<T>(create));
        }

        public static IAsyncEnumerable<T> DefaultIfEmpty<T>(this IAsyncEnumerable<T> source, IAsyncEnumerable<T> defaultObservable)
        {
            Contract.Requires(source != null);
            Contract.Requires(defaultObservable != null);

            return source.Concat(maybe => !maybe.IsSome ? defaultObservable : AsyncEnumerable.Empty<T>());
        }

        public static IAsyncEnumerable<TSource> Defer<TSource>(Func<CancellationToken, Task<IAsyncEnumerable<TSource>>> asyncFactory)
        {
            if (asyncFactory == null)
                throw new ArgumentNullException(nameof(asyncFactory));

            return AsyncEnumerable.CreateEnumerable(() =>
            {
                var baseEnumerator = default(IAsyncEnumerator<TSource>);

                return AsyncEnumerable.CreateEnumerator(
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

        public static IAsyncEnumerable<TSource> Dematerialize<TSource>(this IAsyncEnumerable<Notification<TSource>> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var e = enumerable.GetEnumerator();
                    var current = default(TSource);

                    return AsyncEnumerable.CreateEnumerator(
                        ct =>
                        {
                            return e
                                .MoveNext(ct)
                                .Then(result =>
                                {
                                    if (result)
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
                                });
                        },
                        () => current,
                        e.Dispose);
                });
        }

        public static IAsyncEnumerable<T> Empty<T>(TimeSpan delay)
        {
            return AsyncEnumerable.CreateEnumerable(
                () => AsyncEnumerable.CreateEnumerator<T>(
                    ct => Task
                        .Delay(delay, ct)
                        .Then(() => false),
                    () => { throw new InvalidOperationException(); },
                    null));
        }

        public static IAsyncEnumerable<T> Gate<T>(this IAsyncEnumerable<T> enumerable, Func<CancellationToken, Task> gateTaskFunction)
        {
            Contract.Requires(enumerable != null);
            Contract.Requires(gateTaskFunction != null);

            return AsyncEnumerable
                .CreateEnumerable(() =>
                {
                    var e = enumerable.GetEnumerator();

                    return AsyncEnumerable.CreateEnumerator(
                        ct => gateTaskFunction(ct)
                            .Then(() => e.MoveNext(ct)),
                        () => e.Current,
                        e.Dispose);
                });
        }

        public static IAsyncEnumerable<T> KeepOpen<T>(this IAsyncEnumerable<T> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable.CreateEnumerable(() =>
            {
                var e = enumerable.GetEnumerator();

                return AsyncEnumerable.CreateEnumerator(
                    ct => e
                        .MoveNext(ct)
                        .Then(result =>
                        {
                            if (result)
                                return Task.FromResult(true);

                            e.Dispose();
                            return Task.Factory.GetUncompleted<bool>();
                        }),
                    () => e.Current,
                    e.Dispose);
            });
        }

        public static IAsyncEnumerable<Notification<TSource>> Materialize<TSource>(this IAsyncEnumerable<TSource> enumerable)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var completed = false;
                    var e = enumerable.GetEnumerator();
                    var current = default(Notification<TSource>);

                    return AsyncEnumerable.CreateEnumerator(
                        ct => e
                            .MoveNext(ct)
                            .ContinueWith(task =>
                            {
                                if (completed)
                                    return false;

                                if (task.IsFaulted)
                                {
                                    completed = true;
                                    current = Notification.CreateOnError<TSource>(task.Exception.InnerException);
                                }
                                else if (task.Result)
                                    current = Notification.CreateOnNext(e.Current);
                                else
                                {
                                    completed = true;
                                    current = Notification.CreateOnCompleted<TSource>();
                                }

                                return true;
                            }, TaskContinuationOptions.NotOnCanceled),
                        () => current,
                        e.Dispose);
                });
        }

        public static IAsyncEnumerable<TAccumulate> ScanAsync<TSource, TAccumulate>(this IAsyncEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, CancellationToken, Task<TAccumulate>> accumulator)
        {
            Contract.Requires(source != null);
            Contract.Requires(accumulator != null);

            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var acc = seed;
                    var e = source.GetEnumerator();

                    return AsyncEnumerable.CreateEnumerator(
                        ct => e
                            .MoveNext(ct)
                            .Then(result => result
                                ? accumulator(acc, e.Current, ct)
                                    .Then(newAcc =>
                                    {
                                        acc = newAcc;

                                        return true;
                                    })
                                : Task.FromResult(false)),
                        () => acc,
                        e.Dispose);
                });
        }

        public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, Task<TResult>> selector)
        {
            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var current = default(TResult);
                    var e = enumerable.GetEnumerator();

                    return AsyncEnumerable.CreateEnumerator(
                        async ct =>
                        {
                            if (await e.MoveNext(ct))
                            {
                                current = await selector(e.Current, ct);
                                return true;
                            }

                            return false;
                        },
                        () => current,
                        e.Dispose);
                });
        }

        public static IAsyncEnumerable<Unit> SelectMany<TSource>(this IAsyncEnumerable<TSource> enumerable, Func<TSource, CancellationToken, Task> selector)
        {
            return enumerable
                .SelectMany((x, ct) => selector(x, ct).AsUnitTask());
        }

        public static IAsyncEnumerable<T> SelectValueWhereAvailable<T>(this IAsyncEnumerable<Option<T>> enumerable)
        {
            return enumerable
                .SelectMany(maybe => maybe.ToAsyncEnumerable());
        }

        public static IAsyncEnumerable<TSource> StateScan<TSource, TState>(this IAsyncEnumerable<TSource> source, TState seed, Func<TState, TSource, TState> stateFunction)
        {
            Contract.Requires(source != null);
            Contract.Requires(stateFunction != null);

            return source
                .Scan(
                    Tuple.Create(seed, default(TSource)),
                    (stateTuple, value) => Tuple.Create(stateFunction(stateTuple.Item1, value), value))
                .Select(stateTuple => stateTuple.Item2);
        }

        #region ByteInterfaceStream
        private sealed class JoinStream : Stream
        {
            private readonly IAsyncEnumerator<ArraySegment<byte>> _arraySegmentEnumerator;

            private ArraySegment<byte>? _currentInputSegment;

            public JoinStream(IAsyncEnumerator<ArraySegment<byte>> factory)
            {
                Contract.Requires(factory != null);

                this._arraySegmentEnumerator = factory;
            }

            #region Read
            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.ReadAsync(buffer, offset, count, CancellationToken.None).Result;
            }
            #endregion

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

                ArraySegment<byte> currentInputSegment;
                var currentNullableInputSegment = this._currentInputSegment;

                if (currentNullableInputSegment == null)
                {
                    try
                    {
                        if (await this._arraySegmentEnumerator.MoveNext(CancellationToken.None).ConfigureAwait(false))
                            currentInputSegment = this._arraySegmentEnumerator.Current;
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
                this._currentInputSegment = ((currentInputSegment.Count > 0) ? ((ArraySegment<byte>?)currentInputSegment) : (null));

                return minToRead;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    this._arraySegmentEnumerator.Dispose();

                base.Dispose(disposing);
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotSupportedException();
                }

                set
                {
                    throw new NotSupportedException();
                }
            }
        }
        #endregion

        public static Stream ToStream(this IAsyncEnumerable<ArraySegment<byte>> byteSegmentAsyncEnumerable)
        {
            Contract.Requires(byteSegmentAsyncEnumerable != null);

            return new JoinStream(byteSegmentAsyncEnumerable.GetEnumerator());
        }

        public static Task<Option<T>> TryFirst<T>(this IAsyncEnumerable<T> enumerable)
        {
            return enumerable.TryFirst(CancellationToken.None);
        }

        public static async Task<Option<T>> TryFirst<T>(this IAsyncEnumerable<T> enumerable, CancellationToken token)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                if (await enumerator.MoveNext(token).ConfigureAwait(false))
                    return enumerator.Current;

                return Option<T>.None;
            }
        }

        public static IAsyncEnumerable<T> TryWithTimeout<T>(this IAsyncEnumerable<T> enumerable, TimeSpan timeout)
        {
            Contract.Requires(enumerable != null);

            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var e = enumerable.GetEnumerator();

                    return AsyncEnumerable.CreateEnumerator(
                        async ct =>
                        {
                            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                            {
                                var option = await e
                                    .MoveNext(cts.Token)
                                    .TryWithTimeout(timeout)
                                    .ConfigureAwait(false);

                                if (option.IsNone)
                                    cts.Cancel();

                                return option.IfNone(false);
                            }
                        },
                        () => e.Current,
                        e.Dispose);
                });
        }

        public static IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T> source)
        {
            Contract.Requires(source != null);

            return source.Where(t => !object.Equals(t, default(T)));
        }

        public static IAsyncEnumerable<T> WithCancellation<T>(Func<CancellationToken, IAsyncEnumerable<T>> enumerableFactory)
        {
            Contract.Requires(enumerableFactory != null);

            return AsyncEnumerable
                .Using(
                    () => new CancellationDisposable(),
                    cts => enumerableFactory(cts.Token));
        }
    }
}
