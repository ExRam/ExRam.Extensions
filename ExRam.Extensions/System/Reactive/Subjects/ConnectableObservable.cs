namespace System.Reactive.Subjects
{
    public static partial class ConnectableObservable
    {
        private sealed class FuncConnectableObservable<T> : IConnectableObservable<T>
        {
            private readonly Func<IDisposable> _connectFunction;
            private readonly Func<IObserver<T>, IDisposable> _subscribeFuntion;

            public FuncConnectableObservable(Func<IDisposable> connectFunction, Func<IObserver<T>, IDisposable> subscribeFuntion)
            {
                _connectFunction = connectFunction;
                _subscribeFuntion = subscribeFuntion;
            }

            public IDisposable Connect()
            {
                return _connectFunction();
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                return _subscribeFuntion(observer);
            }
        }

        public static IConnectableObservable<T> Create<T>(Func<IDisposable> connectFunction, Func<IObserver<T>, IDisposable> subscribeFuntion)
        {
            return new FuncConnectableObservable<T>(connectFunction, subscribeFuntion);
        }
    }
}
