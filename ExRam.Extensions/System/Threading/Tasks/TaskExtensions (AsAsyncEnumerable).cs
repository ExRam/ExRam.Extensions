// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this Task<IEnumerable<T>> enumerableTask)
        {
            Contract.Requires(enumerableTask != null);
           
            return enumerableTask
                .ToAsyncEnumerable<IEnumerable<T>>()
                .SelectMany(x => x.ToAsyncEnumerable());
        }
    }
}
