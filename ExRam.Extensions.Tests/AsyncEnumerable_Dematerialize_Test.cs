// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class AsyncEnumerable_Dematerialize_Test
    {
        #region AsyncEnumerable_Dematerialize_handles_OnNext_correctly
        [TestMethod]
        public async Task AsyncEnumerable_Dematerialize_handles_OnNext_correctly()
        {
            var values = await AsyncEnumerable.Range(0, 10)
                .Materialize()
                .Dematerialize()
                .ToArray();

            Assert.AreEqual(10, values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(i, values[i]);
            }
        }
        #endregion

        #region AsyncEnumerable_Materialize_handles_OnError_correctly
        [TestMethod]
        public async Task AsyncEnumerable_Materialize_handles_OnError_correctly()
        {
            var ex = new DivideByZeroException();
            var enumerable = AsyncEnumerable.Range(0, 3)
                .Concat(AsyncEnumerable.Throw<int>(ex))
                .Materialize()
                .Dematerialize();

            using(var e = enumerable.GetEnumerator())
            {
                Assert.IsTrue(await e.MoveNext(CancellationToken.None));
                Assert.AreEqual(0, e.Current);

                Assert.IsTrue(await e.MoveNext(CancellationToken.None));
                Assert.AreEqual(1, e.Current);

                Assert.IsTrue(await e.MoveNext(CancellationToken.None));
                Assert.AreEqual(2, e.Current);
            
                try
                {
                    await e.MoveNext(CancellationToken.None);
                    Assert.Fail();
                }
                catch(AggregateException)
                {
                    
                }
            }
        }
        #endregion
    }
}
