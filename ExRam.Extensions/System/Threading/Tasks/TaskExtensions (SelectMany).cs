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
        #region SelectMany (Task<TSource>)
        public static Task<TResult> SelectMany<TSource, TResult>(this Task<TSource> task, Func<TSource, Task<TResult>> selector)
        {
            Contract.Requires(task != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<TResult>>() != null);

            if (task.IsCompleted)
            {
                switch (task.Status)
                {
                    case (TaskStatus.RanToCompletion):
                    {
                        try
                        {
                            return selector(task.Result);
                        }
                        catch (Exception ex)
                        {
                            return Task.Factory.GetFaulted<TResult>(ex);
                        }
                    }

                    case (TaskStatus.Faulted):
                    {
                        // ReSharper disable PossibleNullReferenceException
                        return Task.Factory.GetFaulted<TResult>(task.Exception.InnerExceptions);
                        // ReSharper restore PossibleNullReferenceException
                    }

                    case (TaskStatus.Canceled):
                    {
                        return Task.Factory.GetCanceled<TResult>();
                    }
                }
            }

            var tcs = new TaskCompletionSource<TResult>();

            task.ContinueWith(task2 =>
            {
                switch (task2.Status)
                {
                    case(TaskStatus.RanToCompletion):
                    {
                        try
                        {
                            selector(task2.Result).ContinueWith(task3 =>
                            {
                                switch (task3.Status)
                                {
                                    case(TaskStatus.RanToCompletion):
                                    {
                                        try
                                        {
                                            tcs.SetResult(task3.Result);
                                        }
                                        catch (Exception ex)
                                        {
                                            tcs.SetException(ex);
                                        }

                                        break;
                                    }

                                    case (TaskStatus.Faulted):
                                    {
                                        // ReSharper disable PossibleNullReferenceException
                                        tcs.SetException(task2.Exception.InnerExceptions);
                                        // ReSharper restore PossibleNullReferenceException
                                        break;
                                    }

                                    case(TaskStatus.Canceled):
                                    {
                                        tcs.SetCanceled();

                                        break;
                                    }
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }

                        break;
                    }

                    case (TaskStatus.Faulted):
                    {
                        // ReSharper disable PossibleNullReferenceException
                        tcs.SetException(task2.Exception.InnerExceptions);
                        // ReSharper restore PossibleNullReferenceException
                        break;
                    }

                    case(TaskStatus.Canceled):
                    {
                        tcs.SetCanceled();

                        break;
                    }
                }
            });

            return tcs.Task;
        }
        #endregion

        #region SelectMany (Task)
        public static Task<TResult> SelectMany<TResult>(this Task task, Func<Task<TResult>> selector)
        {
            Contract.Requires(task != null);
            Contract.Requires(selector != null);
            Contract.Ensures(Contract.Result<Task<TResult>>() != null);

            if (task.IsCompleted)
            {
                switch (task.Status)
                {
                    case (TaskStatus.RanToCompletion):
                    {
                        try
                        {
                            return selector();
                        }
                        catch (Exception ex)
                        {
                            return Task.Factory.GetFaulted<TResult>(ex);
                        }
                    }

                    case (TaskStatus.Faulted):
                    {
                        // ReSharper disable PossibleNullReferenceException
                        return Task.Factory.GetFaulted<TResult>(task.Exception.InnerExceptions);
                        // ReSharper restore PossibleNullReferenceException
                    }

                    case (TaskStatus.Canceled):
                    {
                        return Task.Factory.GetCanceled<TResult>();
                    }
                }
            }

            var tcs = new TaskCompletionSource<TResult>();

            task.ContinueWith(task2 =>
            {
                switch (task2.Status)
                {
                    case (TaskStatus.RanToCompletion):
                    {
                        selector().ContinueWith(task3 =>
                        {
                            switch (task3.Status)
                            {
                                case (TaskStatus.RanToCompletion):
                                {
                                    try
                                    {
                                        tcs.SetResult(task3.Result);
                                    }
                                    catch (Exception ex)
                                    {
                                        tcs.SetException(ex);
                                    }

                                    break;
                                }

                                case (TaskStatus.Faulted):
                                {
                                    // ReSharper disable PossibleNullReferenceException
                                    tcs.SetException(task3.Exception.InnerExceptions);
                                    // ReSharper restore PossibleNullReferenceException
                                    break;
                                }

                                case (TaskStatus.Canceled):
                                {
                                    tcs.SetCanceled();

                                    break;
                                }
                            }
                        });

                        break;
                    }

                    case (TaskStatus.Faulted):
                    {
                        // ReSharper disable PossibleNullReferenceException
                        tcs.SetException(task2.Exception.InnerExceptions);
                        // ReSharper restore PossibleNullReferenceException
                        break;
                    }

                    case (TaskStatus.Canceled):
                    {
                        tcs.SetCanceled();

                        break;
                    }
                }
            });

            return tcs.Task;
        }
        #endregion
    }
}
