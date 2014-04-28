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
        public static void Ignore(this Task task)
        {
            Contract.Requires(task != null);

            // ReSharper disable CSharpWarnings::CS4014
            task.ContinueWith((t) => t.Exception, TaskContinuationOptions.OnlyOnFaulted);
            // ReSharper restore CSharpWarnings::CS4014
        }
    }
}
