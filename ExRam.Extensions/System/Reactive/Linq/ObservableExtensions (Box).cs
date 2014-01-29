using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<object> Box<T>(this IObservable<T> source) where T : struct
        {
            Contract.Requires(source != null);

            return source.Select(x => (object)x);
        }
    }
}
