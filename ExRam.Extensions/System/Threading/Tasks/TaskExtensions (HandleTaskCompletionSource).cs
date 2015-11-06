// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        internal static void HandleTaskCompletionSource<T>(this Task task, TaskCompletionSource<T> tcs, Action<TaskCompletionSource<T>> successAction)
        {
            Contract.Requires(tcs != null);
            Contract.Requires(task != null);
            Contract.Requires(successAction != null);
            Contract.Requires(!task.IsFaulted || task.Exception != null);

            if (task.IsFaulted)
                tcs.TrySetException(task.Exception.InnerExceptions);
            else if (task.IsCanceled)
                tcs.TrySetCanceled();
            else if (task.IsCompleted)
                successAction(tcs);
        }

        internal static void HandleTaskCompletionSource<T1, T2>(this Task<T1> task, TaskCompletionSource<T2> tcs, Action<T1, TaskCompletionSource<T2>> successAction)
        {
            Contract.Requires(tcs != null);
            Contract.Requires(task != null);
            Contract.Requires(successAction != null);
            Contract.Requires(!task.IsFaulted || task.Exception != null);

            if (task.IsFaulted)
                tcs.TrySetException(task.Exception.InnerExceptions);
            else if (task.IsCanceled)
                tcs.TrySetCanceled();
            else if (task.IsCompleted)
                successAction(task.Result, tcs);
        }
    }
}
