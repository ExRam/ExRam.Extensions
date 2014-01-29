using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> KeepOpen<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return source.Concat(Observable.Never<T>());
        }
    }
}
