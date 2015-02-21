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
    public class AsyncEnumerable_SelectMany_Test
    {
        #region AsyncEnumerable_SelectMany_Works
        [TestMethod]
        public async Task AsyncEnumerable_SelectMany_Works()
        {
            var array = await new[] { 1, 2, 3 }.ToAsyncEnumerable()
                .SelectMany(async x =>
                {
                    await Task.Delay(50);
                    return x.ToString();
                })
                .ToArray(CancellationToken.None);

            Assert.AreEqual("1", array[0]);
            Assert.AreEqual("2", array[1]);
            Assert.AreEqual("3", array[2]);
        }
        #endregion

        #region AsyncEnumerable_SelectMany_Calls_Selector_InOrder
        [TestMethod]
        public async Task AsyncEnumerable_SelectMany_Calls_Selector_InOrder()
        {
            var counter = 0;

            var array = await new[] { 1, 2, 3 }.ToAsyncEnumerable()
                .SelectMany(async x =>
                {
                    await Task.Delay(50);
                    Assert.AreEqual(x, Interlocked.Increment(ref counter));

                    return x.ToString();
                })
                .ToArray(CancellationToken.None);

            Assert.AreEqual("1", array[0]);
            Assert.AreEqual("2", array[1]);
            Assert.AreEqual("3", array[2]);
        }
        #endregion
    }
}
