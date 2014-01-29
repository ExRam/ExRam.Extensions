using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        #region Swallow<TException>(Task)
        public static async Task Swallow<TException>(this Task task) where TException : Exception
        {
            Contract.Requires(task != null);

            try
            {
                await task.ConfigureAwait(false);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (TException)
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }
        #endregion

        #region Swallow<TException>(Task<Result>)
        public static async Task<Maybe<TResult>> Swallow<TException, TResult>(this Task<TResult> task) where TException : Exception
        {
            Contract.Requires(task != null);

            try
            {
                return (Maybe<TResult>)(await task.ConfigureAwait(false));
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (TException)
            // ReSharper restore EmptyGeneralCatchClause
            {
                return Maybe<TResult>.Null;
            }
        }
        #endregion

        #region Swallow(Task<TResult>)
        public static async Task<Maybe<TResult>> Swallow<TResult>(this Task<TResult> task)
        {
            Contract.Requires(task != null);

            try
            {
                return (Maybe<TResult>)(await task.ConfigureAwait(false));
            }
            catch
            {
                return Maybe<TResult>.Null;
            }
        }
        #endregion

        #region Swallow(Task)
        public static async Task Swallow(this Task task)
        {
            Contract.Requires(task != null);

            try
            {
                await task.ConfigureAwait(false);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }
        #endregion
    }
}
