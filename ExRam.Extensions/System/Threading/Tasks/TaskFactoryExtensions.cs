// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Threading.Tasks
{
    public static class TaskFactoryExtensions
    {
        #region CreateUncompletedTask
        public static Task<TResult> GetUncompleted<TResult>(this TaskFactory factory)
        {
            return new TaskCompletionSource<TResult>().Task;
        }
        #endregion

        #region CreateFaultedTask
        public static Task<TResult> GetFaulted<TResult>(this TaskFactory factory, Exception exception)
        {
            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetException(exception);

            return completionSource.Task;
        }
        #endregion

        #region CreateCanceledTask
        public static Task<TResult> GetCanceled<TResult>(this TaskFactory factory)
        {
            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetCanceled();

            return completionSource.Task;
        }
        #endregion
    }
}
