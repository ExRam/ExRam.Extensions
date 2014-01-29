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
        #region Select (Task<TSource>)
        public static Task<TResult> Select<TSource, TResult>(this Task<TSource> task, Func<TSource, TResult> selector)
        {
            Contract.Requires(task != null);
            Contract.Requires(selector != null);

            if (task.IsCompleted)
            {
                switch (task.Status)
                {
                    case (TaskStatus.RanToCompletion):
                    {
                        try
                        {
                            return Task.FromResult(selector(task.Result));
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

            task.ContinueWith((task2) =>
            {
                switch (task2.Status)
                {
                    case(TaskStatus.RanToCompletion):
                    {
                        try
                        {
                            tcs.SetResult(selector(task2.Result));
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

        #region Select (Task)
        public static Task<TResult> Select<TResult>(this Task task, Func<TResult> selector)
        {
            Contract.Requires(task != null);
            Contract.Requires(selector != null);

            if (task.IsCompleted)
            {
                switch (task.Status)
                {
                    case (TaskStatus.RanToCompletion):
                    {
                        try
                        {
                            return Task.FromResult(selector());
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

            task.ContinueWith((task2) =>
            {
                switch (task2.Status)
                {
                    case (TaskStatus.RanToCompletion):
                    {
                        try
                        {
                            tcs.SetResult(selector());
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
