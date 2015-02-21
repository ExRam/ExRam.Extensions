// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class AsyncEnumerable_ScanAsync_Test
    {
        [TestMethod]
        public async Task ScanAsync_behaves_like_scan()
        {
            var source = AsyncEnumerable.Range(1, 10);

            var array1 = await source
                .Scan(0, (x, y) => x + y)
                .ToArray();

            var array2 = await source
                .ScanAsync(
                    0,
                    async (x, y, ct) =>
                    {
                        await Task.Delay(5, ct);

                        return x + y;
                    })
                .ToArray();

            CollectionAssert.AreEqual(array1, array2);
        }
    }
}
