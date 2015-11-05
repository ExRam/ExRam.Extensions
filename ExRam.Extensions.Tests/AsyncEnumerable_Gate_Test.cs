// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class AsyncEnumerable_Gate_Test
    {
        #region AsyncEnumerable_Gate_Works
        [TestMethod]
        public async Task AsyncEnumerable_Gate_Works()
        {
            var i = 0;
            var tcs = Enumerable.Range(1, 10).Select(x => new TaskCompletionSource<object>()).ToArray();
            var ae = (new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }).ToAsyncEnumerable();

            var only = ae.Gate((ct) => tcs[i++].Task);

            var enumerator = only.GetEnumerator();

            for (var j = 1; j <= 10; j++)
            {
                var task = enumerator.MoveNext(CancellationToken.None);
                await Task.Delay(50);

                Assert.IsFalse(task.IsCompleted);

                tcs[(j - 1)].SetResult(null);

                Assert.IsTrue(await task);
                Assert.AreEqual(j, enumerator.Current);
            }
        }
        #endregion
    }
}
