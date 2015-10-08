// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static async Task<Unit> AsUnitTask(this Task task)
        {
            Contract.Requires(task != null);

            await task;
            return Unit.Default;
        }
    }
}
