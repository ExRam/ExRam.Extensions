// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

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
    }
}
