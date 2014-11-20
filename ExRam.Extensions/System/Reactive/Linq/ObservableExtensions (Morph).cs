// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Threading;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        private sealed class MorphObservable<T> : IObservable<T>
        {
            private int _currentIndex = -1;
            private readonly IObservable<T>[] _observables;

            public MorphObservable(IObservable<T>[] observables)
            {
                Contract.Requires(observables != null);
                Contract.Requires(Contract.ForAll(observables, observable => observable != null));

                this._observables = observables;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                Contract.Assume(Contract.ForAll(this._observables, observable => observable != null));

                var index = Interlocked.Increment(ref this._currentIndex);

                if (index < this._observables.Length)
                    return this._observables[index].Subscribe(observer);

                return Observable.Throw<T>(new IndexOutOfRangeException()).Subscribe(observer);
            }
        }

        public static IObservable<T> Morph<T>(params IObservable<T>[] observables)
        {
            Contract.Requires(observables != null);
            Contract.Requires(Contract.ForAll(observables, observable => observable != null));

            return new MorphObservable<T>(observables);
        }
    }
}
