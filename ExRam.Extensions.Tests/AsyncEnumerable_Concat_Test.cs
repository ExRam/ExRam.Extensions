// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class AsyncEnumerable_Concat_Test
    {
        [Fact]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_has_values()
        {
            var array = await new[] { 1, 2, 3 }
                .ToAsyncEnumerable()
                .Concat(maybe => maybe
                    .Filter(x => x == 3)
                    .Match(
                        _ => AsyncEnumerable.Return(4), 
                        () => AsyncEnumerable.Return(-1)))
                .ToArray();

            Assert.Equal(new[] { 1, 2, 3, 4 }, array);
        }

        [Fact]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_is_empty()
        {
            var array = await AsyncEnumerable.Empty<int>()
                .Concat(maybe => AsyncEnumerable.Return(!maybe.IsSome ? 1 : 2))
                .ToArray();

            Assert.Equal(new[] { 1 }, array);
        }

        [Fact]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_faults()
        {
            var ex = new Exception();

            var array = await AsyncEnumerable.Throw<int>(ex)
                .Concat(maybe => AsyncEnumerable.Return(1))
                .Materialize()
                .ToArray();

            array
                .Should()
                .HaveCount(1);

            array[0].Exception
                .Should()
                .Be(ex);
        }
    }
}
