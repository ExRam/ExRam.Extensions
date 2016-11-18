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
        public static async Task<T> Using<T, TResource>(Func<TResource> resourceFactory, Func<TResource, Task<T>> taskFactory)
            where TResource : IDisposable
        {
            Contract.Requires(resourceFactory != null);
            Contract.Requires(taskFactory != null);

            using (var res = resourceFactory())
            {
                return await taskFactory(res).ConfigureAwait(false);
            }
        }
    }
}
