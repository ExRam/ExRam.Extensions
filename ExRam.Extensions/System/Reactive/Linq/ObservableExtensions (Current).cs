// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IAsyncEnumerable<T> Current<T>(this IObservable<T> source)
        {
            Contract.Requires(source != null);

            return AsyncEnumerable
                .Using(
                    () => new ReplaySubject<Notification<T>>(1),
                    subject => AsyncEnumerable
                        .Using(
                            () => source
                                .Materialize()
                                .Multicast(subject)
                                .Connect(),
                            _ => AsyncEnumerable
                                .Repeat(Unit.Default)
                                .SelectMany(unit => subject
                                    .FirstAsync()
                                    .ToAsyncEnumerable())
                                .Dematerialize()))
                .Unwrap();
        }
    }
}
