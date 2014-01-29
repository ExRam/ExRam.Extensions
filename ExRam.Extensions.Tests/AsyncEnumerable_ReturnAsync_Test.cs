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
    public class AsyncEnumerable_ToAsyncEnumerable_Test
    {
        #region AsyncEnumerable_ToAsyncEnumerable_Returns_ValueType_Object
        [TestMethod]
        public async Task AsyncEnumerable_ToAsyncEnumerable_Returns_ValueType_Object()
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(50);
                return 1;
            });

            var enumerable = task.ToAsyncEnumerable();

            using (var enumerator = enumerable.GetEnumerator())
            {
                var maybe = await enumerator.MoveNextAsMaybe(CancellationToken.None);

                Assert.IsTrue(maybe.HasValue);
                Assert.AreEqual(1, maybe.Value);

                maybe = await enumerator.MoveNextAsMaybe(CancellationToken.None);
                Assert.IsFalse(maybe.HasValue);
            }
        }
        #endregion

        #region AsyncEnumerable_ToAsyncEnumerable_Returns_RefType_Object
        [TestMethod]
        public async Task AsyncEnumerable_ToAsyncEnumerable_Returns_RefType_Object()
        {
            var task = Task.Run(async () =>
            {
                await Task.Delay(50);
                return "Hallo";
            });

            var enumerable = task.ToAsyncEnumerable();

            using (var enumerator = enumerable.GetEnumerator())
            {
                var maybe = await enumerator.MoveNextAsMaybe(CancellationToken.None);

                Assert.IsTrue(maybe.HasValue);
                Assert.AreEqual("Hallo", maybe.Value);

                maybe = await enumerator.MoveNextAsMaybe(CancellationToken.None);
                Assert.IsFalse(maybe.HasValue);
            }
        }
        #endregion
    }
}
