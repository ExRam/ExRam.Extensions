// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static IAsyncEnumerable<Unit> ToAsyncEnumerable(this Task task)
        {
            Contract.Requires(task != null);

            return AsyncEnumerable.CreateEnumerable(
                () =>
                {
                    var completed = false;

                    return AsyncEnumerable.CreateEnumerator(
                        ct =>
                        {
                            if (completed)
                                return Task.FromResult(false);

                            return task.Then(() =>
                            {
                                completed = true;
                                return true;
                            });
                        },
                        () => Unit.Default,
                        () => { });
                });
        }
    }
}
