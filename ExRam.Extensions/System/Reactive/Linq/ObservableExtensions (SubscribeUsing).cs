using System.Diagnostics.Contracts;
using System.Reactive.Disposables;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IDisposable SubscribeUsing<T>(this IObservable<T> source, IObserver<T> observer)
        {
            Contract.Requires(source != null);
            Contract.Requires(observer != null);

            var baseSubscription = source.Subscribe(observer);

            return Disposable.Create(() =>
            {
                try
                {
                    baseSubscription.Dispose();
                }
                finally
                {
                    var disposableObserver = observer as IDisposable;
                    if (disposableObserver != null)
                        disposableObserver.Dispose();
                }
            });
        }
    }
}
