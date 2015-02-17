// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        #region StrongReferenceDisposable
        private sealed class StrongReferenceDisposable : IDisposable
        {
            private readonly IDisposable _innerDisposable;

            // ReSharper disable NotAccessedField.Local
            private object _reference;
            // ReSharper restore NotAccessedField.Local

            [MethodImpl(MethodImplOptions.NoOptimization)]
            public StrongReferenceDisposable(IDisposable innerDisposable, object reference)
            {
                Contract.Requires(innerDisposable != null);

                this._innerDisposable = innerDisposable;
                this._reference = reference;
            }
            
            [MethodImpl(MethodImplOptions.NoOptimization)]
            public void Dispose()
            {
                this._innerDisposable.Dispose();
                this._reference = null;
            }
        }
        #endregion

        #region WeakObserver
        private class WeakObserver<T> : IObserver<T>
        {
            private readonly IDisposable _baseSubscription;
            private readonly WeakReference _weakObserverReference;

            public WeakObserver(IObservable<T> observable, IObserver<T> observer)
            {
                Contract.Requires(observable != null);

                this._weakObserverReference = new WeakReference(observer);
                this._baseSubscription = observable.Subscribe(this);
            }

            public void OnCompleted()
            {
                var observer = this._weakObserverReference.Target as IObserver<T>;

                if (observer != null)
                    observer.OnCompleted();
                else
                    this._baseSubscription.Dispose();
            }

            public void OnError(Exception error)
            {
                var observer = this._weakObserverReference.Target as IObserver<T>;

                if (observer != null)
                    observer.OnError(error);
                else
                    this._baseSubscription.Dispose();
            }

            public void OnNext(T value)
            {
                var observer = this._weakObserverReference.Target as IObserver<T>;

                if (observer != null) 
                    observer.OnNext(value);
                else
                    this._baseSubscription.Dispose();
            }

            public IDisposable BaseSubscription
            {
                get
                {
                    return this._baseSubscription;
                }
            }
        }
        #endregion

        public static IObservable<T> ToWeakObservable<T>(this IObservable<T> observable)
        {
            return Observable.Create<T>(baseObserver =>
            {
                var weakObserver = new WeakObserver<T>(observable, baseObserver);

                return new StrongReferenceDisposable(weakObserver.BaseSubscription, baseObserver);
            });
        }
    }
}
