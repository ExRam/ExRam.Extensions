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
        public static async Task<Option<TResult>> Swallow<TException, TResult>(this Task<Option<TResult>> task) where TException : Exception
        {
            Contract.Requires(task != null);

            try
            {
                return await task.ConfigureAwait(false);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (TException)
            // ReSharper restore EmptyGeneralCatchClause
            {
                return Option<TResult>.None;
            }
        }

        public static async Task<Option<TResult>> Swallow<TException, TResult>(this Task<TResult> task) where TException : Exception
        {
            Contract.Requires(task != null);

            try
            {
                return await task.ConfigureAwait(false);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (TException)
            // ReSharper restore EmptyGeneralCatchClause
            {
                return Option<TResult>.None;
            }
        }
        #endregion

        #region Swallow(Task<TResult>)
        public static async Task<Option<TResult>> Swallow<TResult>(this Task<Option<TResult>> task)
        {
            Contract.Requires(task != null);

            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                return Option<TResult>.None;
            }
        }

        public static async Task<Option<TResult>> Swallow<TResult>(this Task<TResult> task)
        {
            Contract.Requires(task != null);

            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                return Option<TResult>.None;
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
