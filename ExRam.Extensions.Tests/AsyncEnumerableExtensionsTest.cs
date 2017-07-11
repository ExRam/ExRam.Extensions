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

        [Fact]
        public async Task AsyncEnumerable_Materialize_roundtrip_handles_OnError_correctly()
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

        [Fact]
        public async Task AsyncEnumerable_Dematerialize_handles_empty_enumerable_correctly()
        {
            var enumerator = AsyncEnumerable
                .Empty<Notification<int>>()
                .Dematerialize()
                .GetEnumerator();

            Assert.False(await enumerator.MoveNext(CancellationToken.None));
        }

        [Fact]
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

                Assert.False(task.IsCompleted);

                tcs[(j - 1)].SetResult(null);

                Assert.True(await task);
                Assert.Equal(j, enumerator.Current);
            }
        }

        [Fact]
        public async Task Final_MoveNext_does_not_complete()
        {
            var source = AsyncEnumerable
                .Range(1, 10)
                .KeepOpen();

            using (var e = source.GetEnumerator())
            {
                for (var i = 1; i <= 10; i++)
                {
                    Assert.True(await e.MoveNext(CancellationToken.None));
                    Assert.Equal(i, e.Current);
                }

                var lastTask = e.MoveNext(CancellationToken.None);
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
                .ToArray();

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
                .ToArray();

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
                .Concat(AsyncEnumerable.Throw<int>(ex))
                .Materialize()
                .ToArray();

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

            Assert.Equal(array1, array2);
        }

        [Fact]
        public async Task AsyncEnumerable_SelectMany_Works()
        {
            var array = await new[] { 1, 2, 3 }.ToAsyncEnumerable()
                .SelectMany(async (x, ct) =>
                {
                    await Task.Delay(50, ct);
                    return x.ToString();
                })
                .ToArray(CancellationToken.None);

            Assert.Equal("1", array[0]);
            Assert.Equal("2", array[1]);
            Assert.Equal("3", array[2]);
        }

        [Fact]
        public async Task AsyncEnumerable_SelectMany_Calls_Selector_InOrder()
        {
            var counter = 0;

            var array = await new[] { 1, 2, 3 }.ToAsyncEnumerable()
                .SelectMany(async (x, ct) =>
                {
                    await Task.Delay(50, ct);
                    Assert.Equal(x, Interlocked.Increment(ref counter));

                    return x.ToString();
                })
                .ToArray(CancellationToken.None);

            Assert.Equal("1", array[0]);
            Assert.Equal("2", array[1]);
            Assert.Equal("3", array[2]);
        }

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

        [Fact]
        public async Task Return_and_break()
        {
            var array = await AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await yielder.Return(1);
                    await yielder.Return(2);

                    await yielder.Break();
                })
                .ToArray();

            array.Should().Equal(1, 2);
        }

        [Fact]
        public async Task Return_break_and_return()
        {
            var array = await AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await yielder.Return(1);
                    await yielder.Return(2);

                    await yielder.Break();

                    await yielder.Return(2);
                })
                .ToArray();

            array.Should().Equal(1, 2);
        }

        [Fact]
        public async Task Return_with_delays()
        {
            var array = await AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await Task.Delay(100, ct);
                    await yielder.Return(1);
                    await Task.Delay(100, ct);
                    await yielder.Return(2);

                    await yielder.Break();
                })
                .ToArray();

            array.Should().Equal(1, 2);
        }

        [Fact]
        public async Task Return_without_break()
        {
            var array = await AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await yielder.Return(1);
                    await yielder.Return(2);
                })
                .ToArray();

            array.Should().Equal(1, 2);
        }

        [Fact]
        public async Task Return_without_break_with_delays()
        {
            var array = await AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await Task.Delay(100, ct);
                    await yielder.Return(1);
                    await Task.Delay(100, ct);
                    await yielder.Return(2);
                })
                .ToArray();

            array.Should().Equal(1, 2);
        }

        [Fact]
        public async Task Cancellation_of_MoveNext()
        {
            CancellationToken outerCt;

            var e = AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    outerCt = ct;
                    await Task.Delay(TimeSpan.FromHours(1));
                })
                .GetEnumerator();

            var cts = new CancellationTokenSource();

            var moveNextTask = e.MoveNext(cts.Token);
            await Task.Delay(50);

            outerCt.CanBeCanceled.Should().BeTrue();
            outerCt.IsCancellationRequested.Should().BeFalse();

            cts.Cancel();
            await Task.Delay(50);

            outerCt.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public async Task Cancellation_by_Dispose()
        {
            CancellationToken outerCt;

            var e = AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    outerCt = ct;
                    await Task.Delay(TimeSpan.FromHours(1));
                })
                .GetEnumerator();

            var cts = new CancellationTokenSource();

            var moveNextTask = e.MoveNext(cts.Token);
            await Task.Delay(50);

            outerCt.CanBeCanceled.Should().BeTrue();
            outerCt.IsCancellationRequested.Should().BeFalse();

            e.Dispose();
            await Task.Delay(50);

            outerCt.IsCancellationRequested.Should().BeTrue();
        }

        [Fact(Skip = "Not.")]
        public async Task Finally_is_executed()
        {
            var finallyCalled = false;

            var array = await AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    try
                    {
                        await yielder.Return(1);
                        await yielder.Return(2);

                        await yielder.Break();
                    }
                    finally
                    {
                        finallyCalled = true;
                    }
                })
                .ToArray();

            array.Should().Equal(1, 2);
            await Task.Delay(100);
            finallyCalled.Should().BeTrue();
        }

        [Fact]
        public async Task Exception_bubbles_up()
        {
            var enumerable = AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await yielder.Return(1);
                    await yielder.Return(2);

                    throw new DivideByZeroException();
                });

            enumerable
                .Awaiting(closure => closure.ToArray())
                .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public async Task Exception_bubbles_up_with_delay()
        {
            var enumerable = AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await yielder.Return(1);
                    await yielder.Return(2);

                    await Task.Delay(100, ct);
                    throw new DivideByZeroException();
                });

            enumerable
                .Awaiting(closure => closure.ToArray())
                .ShouldThrow<DivideByZeroException>();
        }

        [Fact]
        public async Task Exception_is_only_thrown_once()
        {
            var enumerable = AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await yielder.Return(1);
                    await yielder.Return(2);

                    throw new DivideByZeroException();
                });

            using (var enumerator = enumerable.GetEnumerator())
            {
                (await enumerator.MoveNext(CancellationToken.None)).Should().BeTrue();
                (await enumerator.MoveNext(CancellationToken.None)).Should().BeTrue();

                enumerator
                    .Awaiting(closure => closure.MoveNext(CancellationToken.None))
                    .ShouldThrow<DivideByZeroException>();

                (await enumerator.MoveNext(CancellationToken.None)).Should().BeFalse();
            }
        }

        [Fact]
        public async Task MoveNext_throws_when_called_in_wrong_state()
        {
            var enumerable = AsyncEnumerableExtensions
                .Create<int>(async (ct, yielder) =>
                {
                    await Task.Delay(-1, ct);
                });

            using (var enumerator = enumerable.GetEnumerator())
            {
                enumerator.MoveNext(CancellationToken.None);

                enumerator
                    .Awaiting(closure => closure.MoveNext(CancellationToken.None))
                    .ShouldThrow<InvalidOperationException>();
            }
        }
    }
}
