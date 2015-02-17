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
        public static IObservable<T> Forever<T>(T value)
        {
            return Observable.Create<T>(observer =>
            {
                observer.OnNext(value);
                return Disposable.Empty;
            });
        }
    }
}
