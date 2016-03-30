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
    public class AsyncEnumerable_KeepOpen_Test
    {
        [Fact]
        public async Task Final_MoveNext_does_not_complete()
        {
            var source = AsyncEnumerable
                .Range(1, 10)
                .KeepOpen();

            using (var e = source.GetEnumerator())
            {
                for (var i = 1; i <= 10; i++)
                {
                    Assert.True(await e.MoveNext(CancellationToken.None));
                    Assert.Equal(i, e.Current);
                }

                var lastTask = e.MoveNext(CancellationToken.None);
                await Task.Delay(200);

                Assert.False(lastTask.IsCompleted);
            }
        }
    }
}
