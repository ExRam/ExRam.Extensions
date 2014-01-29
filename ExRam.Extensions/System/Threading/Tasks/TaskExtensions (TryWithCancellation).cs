using System.Diagnostics.Contracts;
// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        #region TryWithCancellation(Task, CancellationToken)
        public static Task<bool> TryWithCancellation(this Task task, CancellationToken token)
        {
            Contract.Requires(task != null);

            var projectedTask = task.Select(() => true);
            if (projectedTask.IsCompleted)
                return projectedTask;

            if (token.IsCancellationRequested)
                return Task.FromResult(false);
            
            var tcs = new TaskCompletionSource<bool>();
            var registration = token.Register(() => tcs.TrySetResult(false));

            projectedTask.ContinueWith((task2) =>
            {
                registration.Dispose();
                tcs.TrySetFromTask(task2);
            });

            return tcs.Task;
        }
        #endregion

        #region TryWithCancellation(Task<TResult>, CancellationToken)
        public static Task<Maybe<TResult>> TryWithCancellation<TResult>(this Task<TResult> task, CancellationToken token)
        {
            Contract.Requires(task != null);

            var projectedTask = task.Select((x) => (Maybe<TResult>)x);
            if (projectedTask.IsCompleted)
                return projectedTask;

            if (token.IsCancellationRequested)
                return Task.FromResult(Maybe<TResult>.Null);
            
            var tcs = new TaskCompletionSource<Maybe<TResult>>();
            var registration = token.Register(() => tcs.TrySetResult(Maybe<TResult>.Null));

            projectedTask.ContinueWith((task2) =>
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
