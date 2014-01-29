// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<Unit> OnCompletion<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return Observable.Create<Unit>(observer => source.Subscribe(
                (x) =>
                    {
                    },
                observer.OnError,
                () =>
                    {
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    }));
        }
    }
}
