// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        #region WithTimeout(Task, TimeSpan)
        public static async Task WithTimeout(this Task task, TimeSpan timeout)
        {
            Contract.Requires(task != null);

            if (!(await task.TryWithTimeout(timeout).ConfigureAwait(false)))
                throw new TimeoutException();

            await task.ConfigureAwait(false);
        }
        #endregion

        #region WithTimeout(Task<TResult>, TimeSpan)
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            Contract.Requires(task != null);

            if (!(await task.TryWithTimeout(timeout).ConfigureAwait(false)).IsSome)
                throw new TimeoutException();

            return await task.ConfigureAwait(false);
        }
        #endregion 
    }
}
