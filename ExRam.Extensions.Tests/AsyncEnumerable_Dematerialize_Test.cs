// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ExRam.Extensions.Tests
{
    public class AsyncEnumerable_Dematerialize_Test
    {
        #region AsyncEnumerable_Dematerialize_handles_OnNext_correctly
        [Fact]
        public async Task AsyncEnumerable_Dematerialize_handles_OnNext_correctly()
        {
            var values = await AsyncEnumerable.Range(0, 10)
                .Materialize()
                .Dematerialize()
                .ToArray();

            Assert.Equal(10, values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                Assert.Equal(i, values[i]);
            }
        }
        #endregion

        #region AsyncEnumerable_Materialize_handles_OnError_correctly
        [Fact]
        public async Task AsyncEnumerable_Materialize_handles_OnError_correctly()
        {
            var enumerable = AsyncEnumerable.Range(0, 3)
                .Concat(AsyncEnumerable.Throw<int>(new DivideByZeroException()))
                .Materialize()
                .Dematerialize();

            using (var e = enumerable.GetEnumerator())
            {
                Assert.True(await e.MoveNext(CancellationToken.None));
                Assert.Equal(0, e.Current);

                Assert.True(await e.MoveNext(CancellationToken.None));
                Assert.Equal(1, e.Current);

                Assert.True(await e.MoveNext(CancellationToken.None));
                Assert.Equal(2, e.Current);

                e
                    .Awaiting(_ => _.MoveNext(CancellationToken.None))
                    .ShouldThrowExactly<DivideByZeroException>();
            }
        }
        #endregion

        #region AsyncEnumerable_Dematerialize_handles_empty_enumerable_correctly
        [Fact]
        public async Task AsyncEnumerable_Dematerialize_handles_empty_enumerable_correctly()
        {
            var enumerator = AsyncEnumerable
                .Empty<Notification<int>>()
                .Dematerialize()
                .GetEnumerator();
                
            Assert.False(await enumerator.MoveNext(CancellationToken.None));
        }
        #endregion
    }
}
