// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        #region WithCancellation(Task, CancellationToken)
        public static async Task WithCancellation(this Task task, CancellationToken token)
        {
            if (!await task.TryWithCancellation(token).ConfigureAwait(false))
                throw new TaskCanceledException();
        }
        #endregion

        #region WithCancellation(Task<TResult>, CancellationToken)
        public static async Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken token)
        {
            var maybe = await task.TryWithCancellation(token).ConfigureAwait(false);

            return maybe
                .IfNone(() => { throw new TaskCanceledException(); });
        }
        #endregion
    }
}
