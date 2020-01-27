// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Reactive;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static async IAsyncEnumerable<Unit> ToAsyncEnumerable(this Task task)
        {
            await task;

            yield return Unit.Default;
        }
    }
}
