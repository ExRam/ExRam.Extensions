using System.Diagnostics.Contracts;

namespace System.Reactive.Subjects
{
    public static class ConnectableObservable
    {
        #region FuncConnectableObservable
        private sealed class FuncConnectableObservable<T> : IConnectableObservable<T>
        {
            private readonly Func<IDisposable> _connectFunction;
            private readonly Func<IObserver<T>, IDisposable> _subscribeFuntion;

            public FuncConnectableObservable(Func<IDisposable> connectFunction, Func<IObserver<T>, IDisposable> subscribeFuntion)
            {
                Contract.Requires(connectFunction != null);
                Contract.Requires(subscribeFuntion != null);

                this._connectFunction = connectFunction;
                this._subscribeFuntion = subscribeFuntion;
            }

            public IDisposable Connect()
            {
                return this._connectFunction();
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return this._subscribeFuntion(observer);
            }
        }
        #endregion

        public static IConnectableObservable<T> Create<T>(Func<IDisposable> connectFunction, Func<IObserver<T>, IDisposable> subscribeFuntion)
        {
            Contract.Requires(connectFunction != null);
            Contract.Requires(subscribeFuntion != null);

            return new FuncConnectableObservable<T>(connectFunction, subscribeFuntion);
        }
    }
}
