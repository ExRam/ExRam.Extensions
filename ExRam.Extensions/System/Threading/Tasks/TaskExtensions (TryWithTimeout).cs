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
        #region TryWithTimeout(Task, TimeSpan)
        public static async Task<bool> TryWithTimeout(this Task task, TimeSpan timeout)
        {
            Contract.Requires(task != null);
            Contract.Requires(timeout > TimeSpan.Zero);

            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            {
                await task;
                return true;
            }

            return false;
        }
        #endregion

        #region TryWithTimeout(Task<TResult>, TimeSpan)
        public static async Task<Maybe<TResult>> TryWithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            Contract.Requires(task != null);
            Contract.Requires(timeout > TimeSpan.Zero);

            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
                return await task;

            return Maybe<TResult>.Null;
        }
        #endregion 

        #region TryWithTimeout(Task<TResult>, TimeSpan)
        public static async Task<Maybe<TResult>> TryWithTimeout<TResult>(this Task<Maybe<TResult>> task, TimeSpan timeout)
        {
            Contract.Requires(task != null);
            Contract.Requires(timeout > TimeSpan.Zero);

            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
                return await task;

            return Maybe<TResult>.Null;
        }
        #endregion 
    }
}
