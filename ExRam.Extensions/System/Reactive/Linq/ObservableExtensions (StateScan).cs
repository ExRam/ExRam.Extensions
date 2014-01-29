using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<TSource> StateScan<TSource, TState>(this IObservable<TSource> source, TState seed, Func<TState, TSource, TState> stateFunction)
        {
            Contract.Requires(source != null);
            Contract.Requires(stateFunction != null);

            return source
                .Scan(
                    Tuple.Create(seed, default(TSource)),
                    (stateTuple, value) => Tuple.Create(stateFunction(stateTuple.Item1, value), value))
                .Select(stateTuple => stateTuple.Item2);
        }
    }
}
