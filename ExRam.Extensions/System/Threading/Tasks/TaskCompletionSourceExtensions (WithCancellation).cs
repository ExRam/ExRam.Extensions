// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static partial class TaskCompletionSourceExtensions
    {
        public static TaskCompletionSource<TResult> WithCancellation<TResult>(this TaskCompletionSource<TResult> tcs, CancellationToken token)
        {
            Contract.Requires(tcs != null);

            if (token.IsCancellationRequested)
                tcs.TrySetCanceled();
            else
            {
                var registration = token.Register(() => tcs.TrySetCanceled());
                
                // ReSharper disable once MethodSupportsCancellation
                tcs.Task.ContinueWith(task2 => registration.Dispose());
            }

            return tcs;
        }
    }
}
