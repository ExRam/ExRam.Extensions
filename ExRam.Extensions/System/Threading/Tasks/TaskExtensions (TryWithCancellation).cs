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
        public static async Task<bool> TryWithCancellation(this Task task, CancellationToken ct)
        {
            bool ret;
            var tcs = new TaskCompletionSource<bool>();

            using (ct.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs))
            {
                ret = task == await Task
                    .WhenAny(task, tcs.Task)
                    .ConfigureAwait(false);
            }

            if (ret)
                await task.ConfigureAwait(false);

            return ret;
        }
        
        public static async Task<Option<TResult>> TryWithCancellation<TResult>(this Task<TResult> task, CancellationToken ct)
        {
            bool ret;
            var tcs = new TaskCompletionSource<bool>();

            using (ct.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs))
            {
                ret = task == await Task
                    .WhenAny(task, tcs.Task)
                    .ConfigureAwait(false);
            }

            return ret
                ? await task.ConfigureAwait(false)
                : Option<TResult>.None;
        }
    }
}
