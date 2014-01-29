// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        #region GroupedObservableImpl
        private sealed class GroupedObservableImpl<TKey, TSource> : IGroupedObservable<TKey, TSource>
        {
            private readonly TKey _key;
            private readonly IObservable<TSource> _baseObservable;

            public GroupedObservableImpl(IObservable<TSource> baseObservable, TKey key)
            {
                Contract.Requires(baseObservable != null);

                this._key = key;
                this._baseObservable = baseObservable;
            }

            public IDisposable Subscribe(IObserver<TSource> observer)
            {
                return this._baseObservable.Subscribe(observer);
            }

            #if (CONTRACTS_FULL)
            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                Contract.Invariant(this._baseObservable != null);
            }
            #endif

            public TKey Key
            {
                get
                {
                    return this._key;
                }
            }
        }
        #endregion

        public static IGroupedObservable<TKey, TSource> ToGroup<TKey, TSource>(this IObservable<TSource> source, TKey key)
        {
            Contract.Requires(source != null);

            return new GroupedObservableImpl<TKey, TSource>(source, key);
        }
    }
}
