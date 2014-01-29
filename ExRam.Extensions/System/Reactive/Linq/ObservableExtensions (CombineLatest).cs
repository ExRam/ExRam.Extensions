using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<Tuple<TSource1, TSource2>> CombineLatest<TSource1, TSource2>(this IObservable<TSource1> first, IObservable<TSource2> second)
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);

            return first.CombineLatest(second, Tuple.Create);
        }

        public static IObservable<Tuple<TSource1, TSource2, TSource3>> CombineLatest<TSource1, TSource2, TSource3>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3)
        {
            Contract.Requires(source1 != null);
            Contract.Requires(source2 != null);
            Contract.Requires(source3 != null);

            return source1.CombineLatest(source2, source3, Tuple.Create);
        }

        public static IObservable<Tuple<TSource1, TSource2, TSource3, TSource4>> CombineLatest<TSource1, TSource2, TSource3, TSource4>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4)
        {
            Contract.Requires(source1 != null);
            Contract.Requires(source2 != null);
            Contract.Requires(source3 != null);
            Contract.Requires(source4 != null);

            return source1.CombineLatest(source2, source3, source4, Tuple.Create);
        }
    }
}
