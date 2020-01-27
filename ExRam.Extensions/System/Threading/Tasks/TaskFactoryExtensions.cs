// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Threading.Tasks
{
    public static class TaskFactoryExtensions
    {
        public static Task<TResult> GetUncompleted<TResult>(this TaskFactory factory)
        {
            return new TaskCompletionSource<TResult>().Task;
        }
    }
}
