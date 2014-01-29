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

            #if (CONTRACTS_FULL)
            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                Contract.Invariant(this._enumeratorCreationFunction != null);
            }
            #endif
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

            return Create(() => AsyncEnumeratorEx.Create(function, disposeAction));
        }

        public static IAsyncEnumerable<TTarget> CreateUsing<TDisposable, TTarget>(TDisposable disposable, Func<TDisposable, IAsyncEnumerator<TTarget>> enumeratorCreationFunction) where TDisposable : IDisposable
        {
            Contract.Requires(((object)disposable) != null);
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
