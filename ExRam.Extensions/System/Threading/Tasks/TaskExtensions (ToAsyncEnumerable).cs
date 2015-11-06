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

            return AsyncEnumerableExtensions.Create(
               () =>
               {
                   var completed = false;

                   return AsyncEnumerableExtensions.Create(
                       (ct, tcs) =>
                       {
                           if (completed)
                               tcs.TrySetResult(false);
                           else
                           {
                               task
                                   .ContinueWith(closureTask =>
                                   {
                                       if (closureTask.IsFaulted)
                                           tcs.TrySetException(closureTask.Exception.InnerExceptions);
                                       else if (closureTask.IsCanceled)
                                           tcs.TrySetCanceled();
                                       else if (closureTask.IsCompleted)
                                       {
                                           completed = true;
                                           tcs.TrySetResult(true);
                                       }
                                   }, ct);
                           }

                           return tcs.Task;
                       },
                       () => Unit.Default,
                       () => {});
               });
        }
    }
}
