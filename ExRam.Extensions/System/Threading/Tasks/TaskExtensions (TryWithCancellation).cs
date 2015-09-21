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
        #region TryWithCancellation(Task, CancellationToken)
        public static async Task<bool> TryWithCancellation(this Task task, CancellationToken ct)
        {
            Contract.Requires(task != null);

            bool ret;
            var tcs = new TaskCompletionSource<bool>();

            using (ct.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs))
            {
                ret = (task == await Task.WhenAny(task, tcs.Task));
            }

            if (ret)
                await task;

            return ret;
        }
        #endregion

        #region TryWithCancellation(Task<TResult>, CancellationToken)
        public static async Task<OptionStrict<TResult>> TryWithCancellation<TResult>(this Task<TResult> task, CancellationToken ct)
        {
            Contract.Requires(task != null);

            bool ret;
            var tcs = new TaskCompletionSource<bool>();

            using (ct.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs))
            {
                ret = (task == await Task.WhenAny(task, tcs.Task));
            }

            return ret
                ? await task
                : OptionStrict<TResult>.Nothing;
        }
        #endregion

        #region TryWithCancellation(Task<Maybe<TResult>>, CancellationToken)
        public static async Task<OptionStrict<TResult>> TryWithCancellation<TResult>(this Task<OptionStrict<TResult>> task, CancellationToken token)
        {
            var maybe = await task.TryWithCancellation<OptionStrict<TResult>>(token);
            if ((maybe.HasValue) && (maybe.Value.HasValue))
                return maybe.Value.Value;

            return OptionStrict<TResult>.Nothing;
        }
        #endregion
    }
}
