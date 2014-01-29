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
        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            Contract.Requires(task != null);

            if (task.IsCompleted)
                return task;

            var tcs = new TaskCompletionSource<object>();

            task.ContinueWith((task2) =>
            {
                tcs.TrySetFromTask(task2);
            });

            Task.Delay(timeout).ContinueWith((task2) =>
            {
                tcs.TrySetException(new TimeoutException());
            });

            return tcs.Task;
        }
        #endregion

        #region WithTimeout(Task<TResult>, TimeSpan)
        public static Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            Contract.Requires(task != null);

            if (task.IsCompleted)
                return task;

            var tcs = new TaskCompletionSource<TResult>();

            task.ContinueWith((task2) =>
            {
                tcs.TrySetFromTask(task2);
            });

            Task.Delay(timeout).ContinueWith((task2) =>
            {
                tcs.TrySetException(new TimeoutException());
            });

            return tcs.Task;
        }
        #endregion 
    }
}
