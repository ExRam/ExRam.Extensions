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
        #region SetFromTask
        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task task)
        {
            Contract.Requires(tcs != null);
            Contract.Requires(task != null);

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                {
                    tcs.SetResult(default(TResult));
                    break;
                }

                case TaskStatus.Faulted:
                {
                    // ReSharper disable PossibleNullReferenceException
                    tcs.SetException(task.Exception.InnerExceptions);
                    // ReSharper restore PossibleNullReferenceException
                    break;
                }

                case TaskStatus.Canceled:
                {
                    tcs.SetCanceled();
                    break;
                }

                default: 
                    throw new InvalidOperationException("The task was not completed.");
            }
        }

        public static void SetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task<TResult> task)
        {
            Contract.Requires(tcs != null);
            Contract.Requires(task != null);

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                {
                    tcs.SetResult(task.Result);
                    break;
                }

                case TaskStatus.Faulted:
                {
                    // ReSharper disable PossibleNullReferenceException
                    tcs.SetException(task.Exception.InnerExceptions);
                    // ReSharper restore PossibleNullReferenceException
                    break;
                }

                case TaskStatus.Canceled:
                {
                    tcs.SetCanceled();
                    break;
                }

                default: 
                    throw new InvalidOperationException("The task was not completed.");
            }
        }

        public static void SetFromTask<TResult>(this TaskCompletionSource<Maybe<TResult>> tcs, Task<TResult> task)
        {
            Contract.Requires(tcs != null);
            Contract.Requires(task != null);

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                {
                    tcs.SetResult(task.Result);
                    break;
                }

                case TaskStatus.Faulted:
                {
                    // ReSharper disable PossibleNullReferenceException
                    tcs.SetException(task.Exception.InnerExceptions);
                    // ReSharper restore PossibleNullReferenceException
                    break;
                }

                case TaskStatus.Canceled:
                {
                    tcs.SetCanceled();
                    break;
                }

                default:
                    throw new InvalidOperationException("The task was not completed.");
            }
        }
        #endregion

        #region TrySetFromTask
        public static bool TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task task)
        {
            Contract.Requires(tcs != null);
            Contract.Requires(task != null);

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                {
                    return tcs.TrySetResult(default(TResult));
                }

                case TaskStatus.Faulted:
                {
                    // ReSharper disable PossibleNullReferenceException
                    return tcs.TrySetException(task.Exception.InnerExceptions);
                    // ReSharper restore PossibleNullReferenceException
                }

                case TaskStatus.Canceled:
                {
                    return tcs.TrySetCanceled();
                }

                default:
                    throw new InvalidOperationException("The task was not completed.");
            }
        }

        public static bool TrySetFromTask<TResult>(this TaskCompletionSource<TResult> tcs, Task<TResult> task)
        {
            Contract.Requires(tcs != null);
            Contract.Requires(task != null);

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                {
                    return tcs.TrySetResult(task.Result);
                }

                case TaskStatus.Faulted:
                {
                    // ReSharper disable PossibleNullReferenceException
                    return tcs.TrySetException(task.Exception.InnerExceptions);
                    // ReSharper restore PossibleNullReferenceException
                }

                case TaskStatus.Canceled:
                {
                    return tcs.TrySetCanceled();
                }

                default:
                    throw new InvalidOperationException("The task was not completed.");
            }
        }
        #endregion
    }
}
