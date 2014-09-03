// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class AsyncEnumerable_Concat_Test
    {
        #region AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_has_values
        [TestMethod]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_has_values()
        {
            var array = await new[] { 1, 2, 3 }
                .ToAsyncEnumerable()
                .Concat((maybe) =>
                {
                    if (maybe.Value == 3)
                        return AsyncEnumerable.Return(4);

                    return AsyncEnumerable.Return(-1);
                })
                .ToArray();

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, array);
        }
        #endregion

        #region AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_is_empty
        [TestMethod]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_is_empty()
        {
            var array = await AsyncEnumerable.Empty<int>()
                .Concat((maybe) =>
                {
                    if (!maybe.HasValue)
                        return AsyncEnumerable.Return(1);

                    return AsyncEnumerable.Return(2);
                })
                .ToArray();

            CollectionAssert.AreEqual(new[] { 1 }, array);
        }
        #endregion

        #region AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_faults
        [TestMethod]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_faults()
        {
            var ex = new Exception();

            var array = await AsyncEnumerable.Throw<int>(ex)
                .Concat((maybe) => AsyncEnumerable.Return(1))
                .Materialize()
                .ToArray();

            Assert.AreEqual(1, array.Length);
            Assert.AreEqual(ex, array[0].Exception);
        }
        #endregion
    }
}
