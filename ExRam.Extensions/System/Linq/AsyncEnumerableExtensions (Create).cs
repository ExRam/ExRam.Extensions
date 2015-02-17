// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class AsyncEnumerable2
    {
        #region CreateFromEnumeratorFunctionAsyncEnumerable<T>
        private sealed class CreateFromEnumeratorFunctionAsyncEnumerable<T> : IAsyncEnumerable<T>
        {
            private readonly Func<IAsyncEnumerator<T>> _enumeratorCreationFunction;

            public CreateFromEnumeratorFunctionAsyncEnumerable(Func<IAsyncEnumerator<T>> enumeratorCreationFunction)
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

        public static IAsyncEnumerable<T> Create<T>(Func<IAsyncEnumerator<T>> enumeratorCreationFunction)
        {
            Contract.Requires(enumeratorCreationFunction != null);

            return new CreateFromEnumeratorFunctionAsyncEnumerable<T>(enumeratorCreationFunction);
        }

        public static IAsyncEnumerable<T> Create<T>(Func<CancellationToken, Task<Maybe<T>>> function, Action disposeAction = null)
        {
            Contract.Requires(function != null);

            return AsyncEnumerable2.Create(() => AsyncEnumeratorEx.Create(function, disposeAction));
        }

        public static IAsyncEnumerable<TTarget> CreateUsing<TDisposable, TTarget>(TDisposable disposable, Func<TDisposable, IAsyncEnumerator<TTarget>> enumeratorCreationFunction) where TDisposable : IDisposable
        {
            Contract.Requires(disposable != null);
            Contract.Requires(enumeratorCreationFunction != null);

            return AsyncEnumerable2.Create(() =>
            {
                var enumerator = enumeratorCreationFunction(disposable);

                if (enumerator == null)
                    throw new InvalidOperationException();

                return AsyncEnumeratorEx.Create(
                    enumerator.MoveNextAsMaybe,
                    () =>
                    {
                        try
                        {
                            enumerator.Dispose();
                        }
                        finally
                        {
                            disposable.Dispose();
                        }
                    });
            });
        }
    }
}
