// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static async Task WithTimeout(this Task task, TimeSpan timeout, CancellationToken ct)
        {
            if (!await task.TryWithTimeout(timeout, ct).ConfigureAwait(false))
                throw new TimeoutException();

            await task.ConfigureAwait(false);
        }

        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken ct)
        {
            return !(await task.TryWithTimeout(timeout, ct).ConfigureAwait(false)).IsSome 
                ? throw new TimeoutException()
                : await task.ConfigureAwait(false);
        }
    }
}
