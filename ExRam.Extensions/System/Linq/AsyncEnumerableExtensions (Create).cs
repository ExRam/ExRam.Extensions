// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
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
                    if (this._state == YielderState.InFlight)
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

        public static IAsyncEnumerable<T> Create<T>(Func<CancellationToken, IYielder<T>, Task> create)
        {
            if (create == null)
                throw new ArgumentNullException(nameof(create));

            //Wrapping in CreateEnumerable muss sein, da Yielder nur IAsyncEnumerator implementiert und immer neu erzeugt werden muss.
            return AsyncEnumerable.CreateEnumerable(() => new Yielder<T>(create));
        }
    }
}
