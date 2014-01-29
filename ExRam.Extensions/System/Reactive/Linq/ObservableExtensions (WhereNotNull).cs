using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> WhereNotNull<T>(this IObservable<T> source) where T : class
        {
            Contract.Requires(source != null);

            return source.Where(t => !object.Equals(t, default(T)));
        }
    }
}
