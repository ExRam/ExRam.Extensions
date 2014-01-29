// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
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
        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Task<T> task)
        {
            Contract.Requires(task != null);

            return AsyncEnumerable2.Create(() =>
            {
                var called = false;

                return AsyncEnumeratorEx.Create(async (ct) =>
                {
                    if (called)
                        return Maybe<T>.Null;
                 
                    called = true;
                    return await task;
                });
            });
        }
    }
}
