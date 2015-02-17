// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Reactive.Linq;

namespace System.Reactive.Subjects
{
    public sealed class NullableBehaviorSubject<T> : ISubject<T>, IDisposable
    {
        #region Fields
        private readonly BehaviorSubject<Maybe<T>> _innerSubject;
        private readonly IObservable<T> _whereAndSelectInnerObservable;
        #endregion

        #region Constructors
        public NullableBehaviorSubject() : this(Maybe<T>.Null)
        {
        }

        public NullableBehaviorSubject(T value) : this((Maybe<T>)value)
        {
        }

        private NullableBehaviorSubject(Maybe<T> value)
        {
            this._innerSubject = new BehaviorSubject<Maybe<T>>(value);

            this._whereAndSelectInnerObservable = this._innerSubject
                .Where(x => x.HasValue)
                .Select(x => x.Value);
        }
        #endregion

        #region SetNull
        public void SetNull()
        {
            this._innerSubject.OnNext(Maybe<T>.Null);
        }
        #endregion

        #region ISubject<T> implementation
        public void OnCompleted()
        {
            this._innerSubject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            this._innerSubject.OnError(error);
        }

        public void OnNext(T value)
        {
            this._innerSubject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this._whereAndSelectInnerObservable.Subscribe(observer);
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            this._innerSubject.Dispose();
        }
        #endregion
    }
}
