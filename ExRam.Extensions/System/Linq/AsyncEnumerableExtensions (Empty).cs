// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> Empty<T>(TimeSpan delay)
        {
            return AsyncEnumerableExtensions.Create(
                () => AsyncEnumerableExtensions.Create<T>(
                    ct => Task
                        .Delay(delay, ct)
                        .Then(() => false),
                    () => { throw new InvalidOperationException(); },
                    Disposable.Empty));
        }
    }
}
