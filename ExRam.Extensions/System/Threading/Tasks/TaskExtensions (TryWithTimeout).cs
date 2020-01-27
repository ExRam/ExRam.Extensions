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
        public static async Task<bool> TryWithTimeout(this Task task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false))
            {
                await task.ConfigureAwait(false);
                
                return true;
            }

            return false;
        }
        
        public static async Task<Option<TResult>> TryWithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            return task == await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false)
                ? await task.ConfigureAwait(false)
                : Option<TResult>.None;
        }
        
        public static async Task<Option<TResult>> TryWithTimeout<TResult>(this Task<Option<TResult>> task, TimeSpan timeout)
        {
            return task == await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false) 
                ? await task.ConfigureAwait(false)
                : Option<TResult>.None;
        }
    }
}
