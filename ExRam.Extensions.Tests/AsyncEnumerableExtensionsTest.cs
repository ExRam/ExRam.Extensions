// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class AsyncEnumerableExtensionsTest
    {
        [Fact]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_has_values()
        {
            var array = await new[] { 1, 2, 3 }
                .ToAsyncEnumerable()
                .Concat(maybe => maybe
                    .Filter(x => x == 3)
                    .Match(
                        _ => AsyncEnumerableEx.Return(4), 
                        () => AsyncEnumerableEx.Return(-1)))
                .ToArrayAsync();

            Assert.Equal(new[] { 1, 2, 3, 4 }, array);
        }

        [Fact]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_is_empty()
        {
            var array = await AsyncEnumerable.Empty<int>()
                .Concat(maybe => AsyncEnumerableEx.Return(!maybe.IsSome ? 1 : 2))
                .ToArrayAsync();

            Assert.Equal(new[] { 1 }, array);
        }

        [Fact]
        public async Task AsyncEnumerable_Concat_produces_correct_sequence_when_first_sequence_faults()
        {
            var ex = new Exception();

            var array = await AsyncEnumerableEx.Throw<int>(ex)
                .Concat(maybe => AsyncEnumerableEx.Return(1))
                .Materialize()
                .ToArrayAsync();

            array
                .Should()
                .HaveCount(1);

            array[0].Exception
                .Should()
                .Be(ex);
        }

        [Fact]
        public async Task AsyncEnumerable_Dematerialize_handles_OnNext_correctly()
        {
            var values = await AsyncEnumerable.Range(0, 10)
                .Materialize()
                .Dematerialize()
                .ToArrayAsync();

            Assert.Equal(10, values.Length);

            for (var i = 0; i < values.Length; i++)
            {
                Assert.Equal(i, values[i]);
            }
        }

        [Fact]
        public async Task AsyncEnumerable_Materialize_roundtrip_handles_OnError_correctly()
        {
            var enumerable = AsyncEnumerable.Range(0, 3)
                .Concat(AsyncEnumerableEx.Throw<int>(new DivideByZeroException()))
                .Materialize()
                .Dematerialize();

            await using (var e = enumerable.GetAsyncEnumerator())
            {
                Assert.True(await e.MoveNextAsync());
                Assert.Equal(0, e.Current);

                Assert.True(await e.MoveNextAsync());
                Assert.Equal(1, e.Current);

                Assert.True(await e.MoveNextAsync());
                Assert.Equal(2, e.Current);

                e
                    .Awaiting(_ => _.MoveNextAsync().AsTask())
                    .Should()
                    .ThrowExactly<DivideByZeroException>();
            }
        }

        [Fact]
        public async Task AsyncEnumerable_Dematerialize_handles_empty_enumerable_correctly()
        {
            var enumerator = AsyncEnumerable
                .Empty<Notification<int>>()
                .Dematerialize()
                .GetAsyncEnumerator();

            Assert.False(await enumerator.MoveNextAsync());
        }

        [Fact]
        public async Task AsyncEnumerable_Gate_Works()
        {
            var i = 0;
            var tcs = Enumerable.Range(1, 10).Select(x => new TaskCompletionSource<object>()).ToArray();
            var ae = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToAsyncEnumerable();

            var only = ae.Gate((ct) => tcs[i++].Task);

            var enumerator = only.GetAsyncEnumerator();

            for (var j = 1; j <= 10; j++)
            {
                var task = enumerator.MoveNextAsync();
                await Task.Delay(50);

                Assert.False(task.IsCompleted);

                tcs[j - 1].SetResult(null);

                Assert.True(await task);
                Assert.Equal(j, enumerator.Current);
            }
        }

        [Fact(Skip="x")]
        public async Task Final_MoveNext_does_not_complete()
        {
            var source = AsyncEnumerable
                .Range(1, 10)
                .KeepOpen();

            await using (var e = source.GetAsyncEnumerator())
            {
                for (var i = 1; i <= 10; i++)
                {
                    Assert.True(await e.MoveNextAsync());
                    Assert.Equal(i, e.Current);
                }

                var lastTask = e.MoveNextAsync();
                await Task.Delay(200);

                Assert.False(lastTask.IsCompleted);
            }
        }

        [Fact]
        public async Task AsyncEnumerable_Materialize_handles_OnNext_correctly()
        {
            var notifications = await AsyncEnumerable.Range(0, 100)
                .Materialize()
                .Take(10)
                .ToArrayAsync();

            Assert.Equal(10, notifications.Length);
            Assert.True(notifications.All(x => x.Kind == NotificationKind.OnNext));

            for (var i = 0; i < notifications.Length; i++)
            {
                Assert.Equal(i, notifications[i].Value);
            }
        }

        [Fact]
        public async Task AsyncEnumerable_Materialize_handles_OnCompleted_correctly()
        {
            var notifications = await AsyncEnumerable.Range(0, 100)
                .Take(10)
                .Materialize()
                .ToArrayAsync();

            Assert.Equal(11, notifications.Length);

            for (var i = 0; i < notifications.Length - 1; i++)
            {
                Assert.Equal(NotificationKind.OnNext, notifications[i].Kind);
                Assert.Equal(i, notifications[i].Value);
            }

            var lastNotificaton = notifications.Last();

            Assert.Equal(NotificationKind.OnCompleted, lastNotificaton.Kind);
        }

        [Fact]
        public async Task AsyncEnumerable_Materialize_handles_OnError_correctly()
        {
            var ex = new DivideByZeroException();
            var notifications = await AsyncEnumerable.Range(0, 100)
                .Take(10)
                .Concat(AsyncEnumerableEx.Throw<int>(ex))
                .Materialize()
                .ToArrayAsync();

            Assert.Equal(11, notifications.Length);

            for (var i = 0; i < notifications.Length - 1; i++)
            {
                Assert.Equal(NotificationKind.OnNext, notifications[i].Kind);
                Assert.Equal(i, notifications[i].Value);
            }

            var lastNotificaton = notifications.Last();

            Assert.Equal(NotificationKind.OnError, lastNotificaton.Kind);
            Assert.Equal(ex, lastNotificaton.Exception);
        }

        [Fact]
        public async Task ScanAsync_behaves_like_scan()
        {
            var source = AsyncEnumerable.Range(1, 10);

            var array1 = await source
                .Scan(0, (x, y) => x + y)
                .ToArrayAsync();

            var array2 = await source
                .ScanAsync(
                    0,
                    async (x, y, ct) =>
                    {
                        await Task.Delay(5, ct);

                        return x + y;
                    })
                .ToArrayAsync();

            Assert.Equal(array1, array2);
        }

        [Fact]
        public async Task AsyncEnumerable_SelectMany_Works()
        {
            var array = await new[] { 1, 2, 3 }.ToAsyncEnumerable()
                .SelectAwaitWithCancellation(async (x, ct) =>
                {
                    await Task.Delay(50, ct);
                    return x.ToString();
                })
                .ToArrayAsync(CancellationToken.None);

            Assert.Equal("1", array[0]);
            Assert.Equal("2", array[1]);
            Assert.Equal("3", array[2]);
        }

        [Fact]
        public async Task AsyncEnumerable_SelectMany_Calls_Selector_InOrder()
        {
            var counter = 0;

            var array = await new[] { 1, 2, 3 }.ToAsyncEnumerable()
                .SelectAwaitWithCancellation(async (x, ct) =>
                {
                    await Task.Delay(50, ct);
                    Assert.Equal(x, Interlocked.Increment(ref counter));

                    return x.ToString();
                })
                .ToArrayAsync();

            Assert.Equal("1", array[0]);
            Assert.Equal("2", array[1]);
            Assert.Equal("3", array[2]);
        }

        [Fact]
        public async Task AsyncEnumerable_ReadByteAsync_throws_expected_exception()
        {
            var throwingEnumerable = AsyncEnumerableEx.Throw<ArraySegment<byte>>(new IOException());

            var stream = throwingEnumerable.ToStream();

            stream
                .Awaiting(_ => _.ReadAsync(new byte[1], 0, 1))
                .Should()
                .ThrowExactly<IOException>();
        }

        [Fact]
        public async Task AsyncEnumerable_ReadByteAsync_throws_expected_exception1()
        {
            var throwingEnumerable = AsyncEnumerableEx.Throw<ArraySegment<byte>>(new IOException());

            var stream = throwingEnumerable.ToStream();

            stream
                .Awaiting(_ => _.ReadAsync(new byte[1], 0, 1))
                .Should()
                .ThrowExactly<IOException>();
        }
    }
}
