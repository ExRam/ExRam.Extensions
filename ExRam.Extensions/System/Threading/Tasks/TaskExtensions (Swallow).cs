// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using Monad;

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
        public static async Task<OptionStrict<TResult>> Swallow<TException, TResult>(this Task<OptionStrict<TResult>> task) where TException : Exception
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
                return OptionStrict<TResult>.Nothing;
            }
        }

        public static async Task<OptionStrict<TResult>> Swallow<TException, TResult>(this Task<TResult> task) where TException : Exception
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
                return OptionStrict<TResult>.Nothing;
            }
        }
        #endregion

        #region Swallow(Task<TResult>)
        public static async Task<OptionStrict<TResult>> Swallow<TResult>(this Task<OptionStrict<TResult>> task)
        {
            Contract.Requires(task != null);

            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                return OptionStrict<TResult>.Nothing;
            }
        }

        public static async Task<OptionStrict<TResult>> Swallow<TResult>(this Task<TResult> task)
        {
            Contract.Requires(task != null);

            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                return OptionStrict<TResult>.Nothing;
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
