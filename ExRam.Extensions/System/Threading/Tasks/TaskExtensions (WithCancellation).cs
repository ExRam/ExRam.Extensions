namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        #region WithCancellation(Task, CancellationToken)
        public static async Task WithCancellation(this Task task, CancellationToken token)
        {
            if (!(await task.TryWithCancellation(token)))
                throw new TaskCanceledException();
        }
        #endregion

        #region WithCancellation(Task<TResult>, CancellationToken)
        public static async Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken token)
        {
            var maybe = await task.TryWithCancellation(token);

            if (!maybe.HasValue)
                throw new TaskCanceledException();

            return maybe.Value;
        }
        #endregion
    }
}
