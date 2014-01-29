// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        #region WithAsyncCallback
        public static Task<TResult> WithAsyncCallback<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
        {
            Contract.Requires(task != null);

            var tcs = new TaskCompletionSource<TResult>(state);

            task.ContinueWith(task2 =>
            {
                tcs.SetFromTask(task2);

                if (callback != null)
                    callback(tcs.Task);
            });

            return tcs.Task;
        }

        public static Task WithAsyncCallback(this Task task, AsyncCallback callback, object state)
        {
            Contract.Requires(task != null);

            var tcs = new TaskCompletionSource<object>(state);
            task.ContinueWith(task2 =>
            {
                tcs.SetFromTask(task2);

                if (callback != null)
                    callback(tcs.Task);
            });

            return tcs.Task;
        }
        #endregion
    }
}
