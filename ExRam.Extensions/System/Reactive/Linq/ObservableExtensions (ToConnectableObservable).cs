// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Subjects;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        private sealed class ConnectableObservableImpl<T> : IConnectableObservable<T>
        {
            private readonly IObservable<T> _source;
            private readonly Func<IDisposable> _connectFunction;

            public ConnectableObservableImpl(IObservable<T> source, Func<IDisposable> connectFunction)
            {
                Contract.Requires(source != null);
                Contract.Requires(connectFunction != null);

                this._source = source;
                this._connectFunction = connectFunction;
            }

            public IDisposable Connect()
            {
                return this._connectFunction();
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return this._source.Subscribe(observer);
            }
        }

        public static IConnectableObservable<T> ToConnectableObservable<T>(this IObservable<T> source, Func<IDisposable> connectFunction)
        {
            Contract.Requires(source != null);
            Contract.Requires(connectFunction != null);

            return new ConnectableObservableImpl<T>(source, connectFunction);
        }
    }
}
