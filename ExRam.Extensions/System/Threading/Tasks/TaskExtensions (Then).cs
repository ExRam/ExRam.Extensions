// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static Task<T2> Then<T1, T2>(this Task<T1> firstTask, Func<T1, T2> resultSelector)
        {
            var tcs = new TaskCompletionSource<T2>();

            firstTask
                .ContinueWith(closureFirstTask =>
                {
                    if (closureFirstTask.IsFaulted)
                        tcs.TrySetException(closureFirstTask.Exception.InnerExceptions);
                    else if (closureFirstTask.IsCanceled)
                        tcs.TrySetCanceled();
                    else
                    {
                        try
                        {
                            tcs.TrySetResult(resultSelector(closureFirstTask.Result));
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static Task<T> Then<T>(this Task firstTask, Func<T> resultSelector)
        {
            var tcs = new TaskCompletionSource<T>();

            firstTask
                .ContinueWith(closureFirstTask =>
                {
                    if (closureFirstTask.IsFaulted)
                        tcs.TrySetException(closureFirstTask.Exception.InnerExceptions);
                    else if (closureFirstTask.IsCanceled)
                        tcs.TrySetCanceled();
                    else
                    {
                        try
                        {
                            tcs.TrySetResult(resultSelector());
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static Task<T2> Then<T1, T2>(this Task<T1> firstTask, Func<T1, Task<T2>> nextTaskSelector)
        {
            var tcs = new TaskCompletionSource<T2>();

            firstTask
                .ContinueWith(closureFirstTask =>
                {
                    if (closureFirstTask.IsFaulted)
                        tcs.TrySetException(closureFirstTask.Exception.InnerExceptions);
                    else if (closureFirstTask.IsCanceled)
                        tcs.TrySetCanceled();
                    else
                    {
                        try
                        {
                            var nextTask = nextTaskSelector(closureFirstTask.Result);

                            if (nextTask == null)
                                tcs.TrySetCanceled();
                            else
                            {
                                nextTask.ContinueWith(closureNextTask =>
                                {
                                    if (closureNextTask.IsFaulted)
                                        tcs.TrySetException(closureNextTask.Exception.InnerExceptions);
                                    else if (closureNextTask.IsCanceled)
                                        tcs.TrySetCanceled();
                                    else
                                        tcs.TrySetResult(closureNextTask.Result);
                                }, TaskContinuationOptions.ExecuteSynchronously);
                            }
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        public static Task<T> Then<T>(this Task firstTask, Func<Task<T>> nextTaskSelector)
        {
            var tcs = new TaskCompletionSource<T>();

            firstTask
                .ContinueWith(closureFirstTask =>
                {
                    if (closureFirstTask.IsFaulted)
                        tcs.TrySetException(closureFirstTask.Exception.InnerExceptions);
                    else if (closureFirstTask.IsCanceled)
                        tcs.TrySetCanceled();
                    else
                    {
                        try
                        {
                            var nextTask = nextTaskSelector();

                            if (nextTask == null)
                                tcs.TrySetCanceled();
                            else
                            {
                                nextTask.ContinueWith(closureNextTask =>
                                {
                                    if (closureNextTask.IsFaulted)
                                        tcs.TrySetException(closureNextTask.Exception.InnerExceptions);
                                    else if (closureNextTask.IsCanceled)
                                        tcs.TrySetCanceled();
                                    else
                                        tcs.TrySetResult(closureNextTask.Result);
                                }, TaskContinuationOptions.ExecuteSynchronously);
                            }
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }
    }
}
