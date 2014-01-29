using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> Catch<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return source.Catch(Observable.Empty<T>());
        }
    }
}
