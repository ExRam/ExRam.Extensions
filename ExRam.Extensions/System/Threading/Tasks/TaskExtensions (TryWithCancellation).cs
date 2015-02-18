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
        #region TryWithCancellation(Task, CancellationToken)
        public static async Task<bool> TryWithCancellation(this Task task, CancellationToken ct)
        {
            Contract.Requires(task != null);

            bool ret;
            var tcs = new TaskCompletionSource<bool>();

            using (ct.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs))
            {
                ret = (task == await Task.WhenAny(task, tcs.Task));
            }

            if (ret)
                await task;

            return ret;
        }
        #endregion

        #region TryWithCancellation(Task<TResult>, CancellationToken)
        public static Task<Maybe<TResult>>  TryWithCancellation<TResult>(this Task<TResult> task, CancellationToken token)
        {
            Contract.Requires(task != null);

            var projectedTask = task.Select(x => (Maybe<TResult>)x);
            
            if (projectedTask.IsCompleted)
                return projectedTask;

            if (token.IsCancellationRequested)
                return Task.FromResult(Maybe<TResult>.Null);
            
            var tcs = new TaskCompletionSource<Maybe<TResult>>();
            var registration = token.Register(() => tcs.TrySetResult(Maybe<TResult>.Null));

            projectedTask.ContinueWith(task2 =>
            {
                registration.Dispose();
                tcs.TrySetFromTask(task2);
            });

            return tcs.Task;
        }
        #endregion

        #region TryWithCancellation(Task<Maybe<TResult>>, CancellationToken)
        public static async Task<Maybe<TResult>> TryWithCancellation<TResult>(this Task<Maybe<TResult>> task, CancellationToken token)
        {
            var maybe = await task.TryWithCancellation<Maybe<TResult>>(token);
            if ((maybe.HasValue) && (maybe.Value.HasValue))
                return maybe.Value.Value;

            return Maybe<TResult>.Null;
        }
        #endregion
    }
}
