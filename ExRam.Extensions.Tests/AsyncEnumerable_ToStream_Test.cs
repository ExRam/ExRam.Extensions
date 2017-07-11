// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ExRam.Extensions.Tests
{
    public class AsyncEnumerable_ToStream_Test
    {
        [Fact]
        public async Task AsyncEnumerable_ReadByteAsync_throws_expected_exception()
        {
            var throwingEnumerable = AsyncEnumerable.Throw<ArraySegment<byte>>(new IOException());

            var stream = throwingEnumerable.ToStream();

            stream
                .Awaiting(_ => _.ReadAsync(new byte[1], 0, 1))
                .ShouldThrowExactly<IOException>();
        }

        [Fact]
        public async Task AsyncEnumerable_ReadByteAsync_throws_expected_exception1()
        {
            var throwingEnumerable = AsyncEnumerable.Throw<ArraySegment<byte>>(new IOException());

            var stream = throwingEnumerable.ToStream();

            stream
                .Awaiting(_ => _.ReadAsync(new byte[1], 0, 1))
                .ShouldThrowExactly<IOException>();
        }
    }
}
