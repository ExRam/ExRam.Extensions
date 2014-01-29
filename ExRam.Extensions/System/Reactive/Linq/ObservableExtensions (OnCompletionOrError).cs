using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<Exception> OnCompletionOrError<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return source
                .Materialize()
                .Where(x => !x.HasValue)
                .Select(x => x.Exception);
        }
    }
}
