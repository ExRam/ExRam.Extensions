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
        public static IObservable<TResult> UsingWhile<TResult, TResource>(Func<TResource> resourceFactory, Func<TResource, IObservable<TResult>> observableFactory, Predicate<TResult> predicate) where TResource : IDisposable
        {
            Contract.Requires(predicate != null);
            Contract.Requires(resourceFactory != null);
            Contract.Requires(observableFactory != null);

            return Observable.Create<TResult>(observer =>
            {
                var resource = resourceFactory();

                return Observable
                    .Using(
                        () => new SingleAssignmentDisposable
                        {
                            Disposable = resource
                        },
                        disposable => Observable
                            .Using(
                                () => disposable,
                                _ => observableFactory(resource))
                            .Do(value =>
                            {
                                if ((!disposable.IsDisposed) && (!predicate(value)))
                                    disposable.Dispose();
                            }))
                    .Subscribe(observer);
            });
        }
    }
}
