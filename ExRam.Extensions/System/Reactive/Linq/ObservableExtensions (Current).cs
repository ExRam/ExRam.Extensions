// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IAsyncEnumerable<T> Current<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return AsyncEnumerable.Using(
                () => new BehaviorSubject<Notification<T>>(null),
                subject => AsyncEnumerable.Using(
                    source.Materialize().KeepOpen().Multicast(subject).Connect,
                    connection => AsyncEnumerable
                        .Repeat(Unit.Default)
                        .SelectAsync(x => subject.WhereNotNull().FirstAsync().ToTask())
                        .Dematerialize()));
        }
    }
}
