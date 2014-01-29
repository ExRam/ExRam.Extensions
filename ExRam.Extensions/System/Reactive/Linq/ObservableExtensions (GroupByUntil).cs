// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        #region EqualityComparerImpl
        private sealed class EqualityComparerImpl<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _comparer;
            private readonly Func<T, int> _hashCodeProvider;

            public EqualityComparerImpl(Func<T, T, bool> comparer, Func<T, int> hashCodeProvider)
            {
                Contract.Requires(comparer != null);
                Contract.Requires(hashCodeProvider != null);

                this._comparer = comparer;
                this._hashCodeProvider = hashCodeProvider;
            }

            public bool Equals(T x, T y)
            {
                return this._comparer(x, y);
            }

            public int GetHashCode(T obj)
            {
                return this._hashCodeProvider(obj);
            }

            #if (CONTRACTS_FULL)
            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                Contract.Invariant(this._comparer != null);
                Contract.Invariant(this._hashCodeProvider != null);
            }
            #endif
        }
        #endregion

        public static IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector, Func<TKey, TKey, bool> comparer, Func<TKey, int> hashCodeProvider)
        {
            Contract.Requires(source != null);
            Contract.Requires(comparer != null);
            Contract.Requires(hashCodeProvider != null);

            return source.GroupByUntil(keySelector, durationSelector, new EqualityComparerImpl<TKey>(comparer, hashCodeProvider));
        }
    }
}
