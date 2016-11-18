// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Reactive.Disposables;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<TSource> TakeWhileInclusive<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return source
                .TakeWhileInclusive((x, i) => predicate(x));
        }

        public static IObservable<TSource> TakeWhileInclusive<TSource>(this IObservable<TSource> source, Func<TSource, int, bool> predicate)
        {
            return Observable.Using(
                () => new SingleAssignmentDisposable(),
                disposable => Observable.Create<TSource>(
                    o =>
                    {
                        var index = 0;

                        return disposable.Disposable = source
                            .Subscribe(
                            x =>
                            {
                                o.OnNext(x);
                                if (!predicate(x, checked(index++)))
                                    o.OnCompleted();
                            },
                            o.OnError,
                            o.OnCompleted);
                    }));
        }
    }
}
