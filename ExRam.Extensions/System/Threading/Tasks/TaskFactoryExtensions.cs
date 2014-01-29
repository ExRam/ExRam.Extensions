// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static class TaskFactoryExtensions
    {
        private static readonly Task CompletedTask;
        private static readonly Task UncompletedTask;

        static TaskFactoryExtensions()
        {
            #region Create Completed Task
            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetResult(null);

            TaskFactoryExtensions.CompletedTask = completionSource.Task;
            TaskFactoryExtensions.UncompletedTask = new TaskCompletionSource<object>().Task;
            #endregion
        }

        #region CreateCompletedTask
        public static Task GetCompleted(this TaskFactory factory)
        {
            return TaskFactoryExtensions.CompletedTask;
        }
        #endregion

        #region CreateUncompletedTask
        public static Task GetUncompleted(this TaskFactory factory)
        {
            return TaskFactoryExtensions.UncompletedTask;
        }

        public static Task<TResult> GetUncompleted<TResult>(this TaskFactory factory)
        {
            return new TaskCompletionSource<TResult>().Task;
        }
        #endregion

        #region CreateFaultedTask
        public static Task GetFaulted(this TaskFactory factory, Exception exception)
        {
            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetException(exception);

            return completionSource.Task;
        }

        public static Task GetFaulted(this TaskFactory factory, IEnumerable<Exception> exceptions)
        {
            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetException(exceptions);

            return completionSource.Task;
        }

        public static Task<TResult> GetFaulted<TResult>(this TaskFactory factory, Exception exception)
        {
            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetException(exception);

            return completionSource.Task;
        }

        public static Task<TResult> GetFaulted<TResult>(this TaskFactory factory, IEnumerable<Exception> exceptions)
        {
            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetException(exceptions);

            return completionSource.Task;
        }
        #endregion

        #region CreateCanceledTask
        public static Task GetCanceled(this TaskFactory factory)
        {
            var completionSource = new TaskCompletionSource<object>();
            completionSource.SetCanceled();

            return completionSource.Task;
        }

        public static Task<TResult> GetCanceled<TResult>(this TaskFactory factory)
        {
            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetCanceled();

            return completionSource.Task;
        }
        #endregion
    }
}
