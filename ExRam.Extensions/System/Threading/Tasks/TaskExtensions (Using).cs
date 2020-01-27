// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static async Task<T> Using<T, TResource>(Func<TResource> resourceFactory, Func<TResource, Task<T>> taskFactory)
            where TResource : IDisposable
        {
            using (var res = resourceFactory())
            {
                return await taskFactory(res).ConfigureAwait(false);
            }
        }
    }
}
