using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        #region TryWithTimeout(Task, TimeSpan)
        public static Task<bool> TryWithTimeout(this Task task, TimeSpan timeout)
        {
            Contract.Requires(task != null);

            var projectedTask = task.Select(() => true);
            if (projectedTask.IsCompleted)
                return projectedTask;

            var tcs = new TaskCompletionSource<bool>();

            projectedTask.ContinueWith((task2) => tcs.TrySetFromTask(task2));
            Task.Delay(timeout).ContinueWith((task2) => tcs.TrySetResult(false));

            return tcs.Task;
        }
        #endregion

        #region TryWithTimeout(Task<TResult>, TimeSpan)
        public static Task<Maybe<TResult>> TryWithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            Contract.Requires(task != null);

            var projectedTask = task.Select(x => (Maybe<TResult>)x);
            if (projectedTask.IsCompleted)
                return projectedTask;

            var tcs = new TaskCompletionSource<Maybe<TResult>>();

            projectedTask.ContinueWith((task2) => tcs.TrySetFromTask(task2));
            Task.Delay(timeout).ContinueWith((task2) => tcs.TrySetResult(Maybe<TResult>.Null));

            return tcs.Task;
        }
        #endregion 

        #region TryWithTimeout(Task<TResult>, TimeSpan)
        public static Task<Maybe<TResult>> TryWithTimeout<TResult>(this Task<Maybe<TResult>> task, TimeSpan timeout)
        {
            Contract.Requires(task != null);

            if (task.IsCompleted)
                return task;

            var tcs = new TaskCompletionSource<Maybe<TResult>>();

            task.ContinueWith((task2) => tcs.TrySetFromTask(task2));
            Task.Delay(timeout).ContinueWith((task2) => tcs.TrySetResult(Maybe<TResult>.Null));

            return tcs.Task;
        }
        #endregion 
    }
}
