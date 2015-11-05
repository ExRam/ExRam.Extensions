// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class AsyncEnumerable_KeepOpen_Test
    {
        [TestMethod]
        public async Task Final_MoveNext_does_not_complete()
        {
            var source = AsyncEnumerable
                .Range(1, 10)
                .KeepOpen();

            using (var e = source.GetEnumerator())
            {
                for (var i = 1; i <= 10; i++)
                {
                    Assert.IsTrue(await e.MoveNext(CancellationToken.None));
                    Assert.AreEqual(i, e.Current);
                }

                var lastTask = e.MoveNext(CancellationToken.None);
                await Task.Delay(200);

                Assert.IsFalse(lastTask.IsCompleted);
            }
        }
    }
}
