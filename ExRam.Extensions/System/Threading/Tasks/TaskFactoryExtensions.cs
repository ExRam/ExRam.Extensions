// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.
using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static class TaskFactoryExtensions
    {
        #region CreateUncompletedTask
        public static Task<TResult> GetUncompleted<TResult>(this TaskFactory factory)
        {
            Contract.Requires(factory != null);
            Contract.Ensures(Contract.Result<Task>() != null);

            return new TaskCompletionSource<TResult>().Task;
        }
        #endregion

        #region CreateFaultedTask
        public static Task<TResult> GetFaulted<TResult>(this TaskFactory factory, Exception exception)
        {
            Contract.Requires(factory != null);
            Contract.Requires(exception != null);
            Contract.Ensures(Contract.Result<Task<TResult>>() != null);

            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetException(exception);

            return completionSource.Task;
        }
        #endregion

        #region CreateCanceledTask
        public static Task<TResult> GetCanceled<TResult>(this TaskFactory factory)
        {
            Contract.Requires(factory != null);
            Contract.Ensures(Contract.Result<Task<TResult>>() != null);

            var completionSource = new TaskCompletionSource<TResult>();
            completionSource.SetCanceled();

            return completionSource.Task;
        }
        #endregion
    }
}
