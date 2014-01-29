using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<Counting<T>> ToCounting<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return source.Select((x, i) => new Counting<T>((ulong)i, x)); 
        }
    }
}
