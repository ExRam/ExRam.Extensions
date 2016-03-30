// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ExRam.Extensions.Tests
{
    public class Task_ToAsyncEnumerable_Test
    {
        [Fact]
        public async Task ToAsyncEnumerable_completes()
        {
            var tcs = new TaskCompletionSource<bool>();

            var task = ((Task)tcs.Task).ToAsyncEnumerable().First();

            Assert.False(task.IsCompleted);
            tcs.SetResult(true);

            await task;
        }

        [Fact]
        public async Task ToAsyncEnumerable_forwards_exception()
        {
            var tcs = new TaskCompletionSource<bool>();

            var task = ((Task)tcs.Task)
                .ToAsyncEnumerable()
                .First();

            Assert.False(task.IsCompleted);
            tcs.SetException(new DivideByZeroException());

            task
                .Awaiting(_ => _)
                .ShouldThrowExactly<AggregateException>()
                .Where(ex => ex.GetBaseException() is DivideByZeroException);
        }
    }
}
