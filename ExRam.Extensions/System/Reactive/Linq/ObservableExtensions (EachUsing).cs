// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Disposables;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> EachUsing<T>(this IObservable<T> source, Func<T, IDisposable> resourceFactory)
        {
            Contract.Requires(source != null);
            Contract.Requires(resourceFactory != null);

            return Observable.Create<T>(obs =>
            {
                IDisposable resource = null;
                var syncRoot = new object();

                var subscription = source.Subscribe(
                    value =>
                    {
                        lock (syncRoot)
                        {
                            resource?.Dispose();
                            resource = resourceFactory(value);
                        }

                        obs.OnNext(value);
                    },
                    ex =>
                    {
                        lock (syncRoot)
                        {
                            resource?.Dispose();
                            resource = null;
                        }

                        obs.OnError(ex);

                    },
                    () =>
                    {
                        lock (syncRoot)
                        {
                            resource?.Dispose();
                            resource = null;
                        }

                        obs.OnCompleted();
                    });

                return StableCompositeDisposable.Create(
                    subscription,
                    Disposable.Create(() =>
                    {
                        lock (syncRoot)
                        {
                            resource?.Dispose();
                            resource = null;
                        }
                    }));
            });
        }
    }
}
