// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static partial class AsyncEnumeratorEx
    {
        #region CreateFromNextFunctionAsyncEnumerator
        private sealed class CreateFromNextFunctionAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly Action _disposeAction;
            private readonly Func<CancellationToken, Task<Maybe<T>>> _function;

            private Maybe<T> _currentItem;

            public CreateFromNextFunctionAsyncEnumerator(Func<CancellationToken, Task<Maybe<T>>> function, Action disposeAction)
            {
                Contract.Requires(function != null);

                this._function = function;
                this._disposeAction = disposeAction;
            }

            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                this._currentItem = await this._function(cancellationToken);
                return this._currentItem.HasValue;
            }

            public void Dispose()
            {
                if (this._disposeAction != null)
                    this._disposeAction();
            }

            public T Current
            {
                get 
                {
                    return this._currentItem.Value;
                }
            }
        }
        #endregion

        public static IAsyncEnumerator<T> Create<T>(Func<CancellationToken, Task<Maybe<T>>> function, Action disposeAction = null)
        {
            Contract.Requires(function != null);

            return new CreateFromNextFunctionAsyncEnumerator<T>(function, disposeAction);
        }

        public static IAsyncEnumerator<T> Create<T>(Func<CancellationToken, Task<Maybe<T>>> function, IDisposable disposable)
        {
            Contract.Requires(function != null);
            Contract.Requires(disposable != null);

            return new CreateFromNextFunctionAsyncEnumerator<T>(function, disposable.Dispose);
        }
    }
}
