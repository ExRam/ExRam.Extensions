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
        public static async Task<bool> TryWithTimeout(this Task task, TimeSpan timeout, CancellationToken ct)
        {
            var delayTask = Task.Delay(timeout, ct);

            if (task == await Task.WhenAny(task, delayTask).ConfigureAwait(false))
            {
                await task.ConfigureAwait(false);
                
                return true;
            }

            await delayTask.ConfigureAwait(false);
            return false;
        }
        
        public static async Task<Option<TResult>> TryWithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken ct)
        {
            var delayTask = Task.Delay(timeout, ct);

            if (task == await Task.WhenAny(task, delayTask).ConfigureAwait(false))
                return await task.ConfigureAwait(false);

            await delayTask.ConfigureAwait(false);
            return Option<TResult>.None;
        }
        
        public static async Task<Option<TResult>> TryWithTimeout<TResult>(this Task<Option<TResult>> task, TimeSpan timeout, CancellationToken ct)
        {
            var delayTask = Task.Delay(timeout, ct);

            if (task == await Task.WhenAny(task, delayTask).ConfigureAwait(false))
                return await task.ConfigureAwait(false);

            await delayTask.ConfigureAwait(false);
            return Option<TResult>.None;
        }
    }
}
