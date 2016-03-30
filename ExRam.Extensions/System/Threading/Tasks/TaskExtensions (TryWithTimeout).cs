// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using LanguageExt;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        #region TryWithTimeout(Task, TimeSpan)
        public static async Task<bool> TryWithTimeout(this Task task, TimeSpan timeout)
        {
            Contract.Requires(task != null);
            Contract.Requires(timeout > TimeSpan.Zero);

            if (task == await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false))
            {
                await task;
                return true;
            }

            return false;
        }
        #endregion

        #region TryWithTimeout(Task<TResult>, TimeSpan)
        public static async Task<Option<TResult>> TryWithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            Contract.Requires(task != null);
            Contract.Requires(timeout > TimeSpan.Zero);

            if (task == await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false))
                return await task;

            return Option<TResult>.None;
        }
        #endregion 

        #region TryWithTimeout(Task<TResult>, TimeSpan)
        public static async Task<Option<TResult>> TryWithTimeout<TResult>(this Task<Option<TResult>> task, TimeSpan timeout)
        {
            Contract.Requires(task != null);
            Contract.Requires(timeout > TimeSpan.Zero);

            if (task == await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false))
                return await task;

            return Option<TResult>.None;
        }
        #endregion 
    }
}
