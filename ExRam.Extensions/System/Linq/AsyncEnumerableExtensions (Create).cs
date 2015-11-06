// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        #region FunctionAsyncEnumerable<T>
        private sealed class FunctionAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly Func<IAsyncEnumerator<T>> _enumeratorCreationFunction;

            public FunctionAsyncEnumerable(Func<IAsyncEnumerator<T>> enumeratorCreationFunction)
            {
                Contract.Requires(enumeratorCreationFunction != null);

                this._enumeratorCreationFunction = enumeratorCreationFunction;
            }

            public IAsyncEnumerator<T> GetEnumerator()
            {
                return this._enumeratorCreationFunction();
            }
        }
        #endregion

        #region FunctionAsyncEnumerator<T>
        private sealed class FunctionAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IDisposable _disposable;
            private readonly Func<T> _currentFunction;
            private readonly Func<CancellationToken, Task<bool>> _moveNextFunction;

            private bool _isDisposed;

            public FunctionAsyncEnumerator(Func<CancellationToken, Task<bool>> moveNextFunction, Func<T> currentFunction, IDisposable disposable)
            {
                Contract.Requires(moveNextFunction != null);
                Contract.Requires(currentFunction != null);
                Contract.Requires(disposable != null);

                this._currentFunction = currentFunction;
                this._disposable = disposable;
                this._moveNextFunction = moveNextFunction;
            }

            public Task<bool> MoveNext(CancellationToken ct)
            {
                return this._isDisposed 
                    ? AsyncEnumerableExtensions.FalseTask
                    : this._moveNextFunction(ct);
            }

            public T Current => this._currentFunction();

            public void Dispose()
            {
                if (!this._isDisposed)
                {
                    this._isDisposed = true;
                    this._disposable.Dispose();
                }
            }
        }
        #endregion

        private static readonly Task<bool> TrueTask = Task.FromResult(true);
        private static readonly Task<bool> FalseTask = Task.FromResult(false);

        public static IAsyncEnumerable<T> Create<T>(Func<IAsyncEnumerator<T>> enumeratorCreationFunction)
        {
            Contract.Requires(enumeratorCreationFunction != null);

            return new FunctionAsyncEnumerable<T>(enumeratorCreationFunction);
        }

        public static IAsyncEnumerator<T> Create<T>(Func<CancellationToken, Task<bool>> moveNextFunction, Func<T> currentFunction, Action disposeFunction)
        {
            Contract.Requires(moveNextFunction != null);
            Contract.Requires(currentFunction != null);
            Contract.Requires(disposeFunction != null);

            return AsyncEnumerableExtensions.Create(moveNextFunction, currentFunction, Disposable.Create(disposeFunction));
        }

        public static IAsyncEnumerator<T> Create<T>(Func<CancellationToken, Task<bool>> moveNextFunction, Func<T> currentFunction, IDisposable disposable)
        {
            Contract.Requires(moveNextFunction != null);
            Contract.Requires(currentFunction != null);
            Contract.Requires(disposable != null);

            return new FunctionAsyncEnumerator<T>(moveNextFunction, currentFunction, disposable );
        }

        public static IAsyncEnumerator<T> Create<T>(Func<CancellationToken, TaskCompletionSource<bool>, Task<bool>> moveNextFunction, Func<T> currentFunction, Action disposeFunction)
        {
            Contract.Requires(moveNextFunction != null);
            Contract.Requires(currentFunction != null);
            Contract.Requires(disposeFunction != null);

            return AsyncEnumerableExtensions.Create(moveNextFunction, currentFunction, Disposable.Create(disposeFunction));
        }

        public static IAsyncEnumerator<T> Create<T>(Func<CancellationToken, TaskCompletionSource<bool>, Task<bool>> moveNextFunction, Func<T> currentFunction, IDisposable disposable)
        {
            Contract.Requires(moveNextFunction != null);
            Contract.Requires(currentFunction != null);
            Contract.Requires(disposable != null);

            var cts = new CancellationDisposable();
            var d = new CompositeDisposable(disposable, cts);

            return new FunctionAsyncEnumerator<T>(
                async ct =>
                {
                    var tcs = new TaskCompletionSource<bool>();

                    var registration = ct.Register(() =>
                    {
                        d.Dispose();
                        tcs.TrySetCanceled();
                    });

                    using (registration)
                    {
                        return await moveNextFunction(cts.Token, tcs).ConfigureAwait(false);
                    }
                },
                currentFunction,
                d);
        }
    }
}
