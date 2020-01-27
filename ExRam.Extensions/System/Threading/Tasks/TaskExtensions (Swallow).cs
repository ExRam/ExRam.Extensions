// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using LanguageExt;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static async Task Swallow<TException>(this Task task) where TException : Exception
        {
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
        
        public static async Task<Option<TResult>> Swallow<TException, TResult>(this Task<Option<TResult>> task) where TException : Exception
        {
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
        
        public static async Task<Option<TResult>> Swallow<TResult>(this Task<Option<TResult>> task)
        {
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
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                return Option<TResult>.None;
            }
        }
        
        public static async Task Swallow(this Task task)
        {
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
    }
}
