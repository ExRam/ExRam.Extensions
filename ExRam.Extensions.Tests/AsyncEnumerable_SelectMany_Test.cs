// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class AsyncEnumerable_SelectMany_Test
    {
        [Fact]
        public async Task AsyncEnumerable_SelectMany_Works()
        {
            var array = await new[] { 1, 2, 3 }.ToAsyncEnumerable()
                .SelectMany(async (x, ct) =>
                {
                    await Task.Delay(50, ct);
                    return x.ToString();
                })
                .ToArray(CancellationToken.None);

            Assert.Equal("1", array[0]);
            Assert.Equal("2", array[1]);
            Assert.Equal("3", array[2]);
        }

        [Fact]
        public async Task AsyncEnumerable_SelectMany_Calls_Selector_InOrder()
        {
            var counter = 0;

            var array = await new[] { 1, 2, 3 }.ToAsyncEnumerable()
                .SelectMany(async (x, ct) =>
                {
                    await Task.Delay(50, ct);
                    Assert.Equal(x, Interlocked.Increment(ref counter));

                    return x.ToString();
                })
                .ToArray(CancellationToken.None);

            Assert.Equal("1", array[0]);
            Assert.Equal("2", array[1]);
            Assert.Equal("3", array[2]);
        }
    }
}
