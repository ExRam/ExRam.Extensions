﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class ObservableExtensionsTests
    {
        [Fact]
        public async Task Observable_Current_blocks_if_no_current_element_is_present()
        {
            var subject = new Subject<int>();
            var asyncEnumerable = subject.Current();

            await using (var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
            {
                var moveNextTask = asyncEnumerator.MoveNextAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(200));

                Assert.False(moveNextTask.IsCompleted);
                Assert.False(moveNextTask.IsCanceled);

                subject.OnNext(1);

                await moveNextTask;

                Assert.True(await asyncEnumerator.MoveNextAsync());
                Assert.Equal(1, asyncEnumerator.Current);
            }
        }

        [Fact]
        public async Task Observable_Current_returns_latest_element()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            await using (var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
            {
                subject.OnNext(2);
                subject.OnNext(3);
                subject.OnNext(4);

                Assert.True(await asyncEnumerator.MoveNextAsync());
                Assert.Equal(4, asyncEnumerator.Current);
            }
        }

        [Fact]
        public async Task Observable_Current_returns_elements_repeatedly()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            await using (var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
            {
                for (var i = 0; i < 10; i++)
                {
                    Assert.True(await asyncEnumerator.MoveNextAsync());
                    Assert.Equal(1, asyncEnumerator.Current);
                }

                subject.OnNext(2);

                for (var i = 0; i < 10; i++)
                {
                    Assert.True(await asyncEnumerator.MoveNextAsync());
                    Assert.Equal(2, asyncEnumerator.Current);
                }
            }
        }

        [Fact]
        public async Task Observable_Current_propagates_exception()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            await using (var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
            {
                subject.OnNext(1);
                subject.OnNext(2);
                subject.OnNext(3);

                subject.OnError(new DivideByZeroException());

                asyncEnumerator
                    .Awaiting(_ => _.MoveNextAsync().AsTask())
                    .Should()
                    .ThrowExactly<DivideByZeroException>();
            }
        }

        [Fact]
        public async Task Observable_Current_completes_on_source_completion()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            await using (var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
            {
                subject.OnNext(1);
                subject.OnNext(2);
                subject.OnNext(3);

                subject.OnCompleted();

                Assert.False(await asyncEnumerator.MoveNextAsync());
            }
        }

        [Fact(Skip="Stimmt in C# 8 nicht.")]
        public async Task Observable_Current_enumerator_disposal_cancels_moveNext()
        {
            var asyncEnumerable = Observable.Never<int>().Current();

            ValueTask<bool> moveNextTask;

            await using (var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator())
            {
                moveNextTask = asyncEnumerator.MoveNextAsync();
            }

            moveNextTask
                .Awaiting(_ => _.AsTask())
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Observable_Current_MoveNext_cancellation_is_effective()
        {
            var asyncEnumerable = Observable.Never<int>().Current();

            var cts = new CancellationTokenSource();

            await using (var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator(cts.Token))
            {
                var moveNextTask = asyncEnumerator.MoveNextAsync();

                await Task.Delay(TimeSpan.FromMilliseconds(200));
                Assert.False(moveNextTask.IsCanceled);
                Assert.False(moveNextTask.IsCompleted);

                cts.Cancel();

                moveNextTask
                    .Awaiting(_ => _.AsTask())
                    .Should()
                    .ThrowExactly<TaskCanceledException>();
            }
        }

        [Fact]
        public async Task Observable_Concat_produces_correct_sequence_when_first_sequence_has_values()
        {
            var array = await new[] { 1, 2, 3 }
                .ToObservable()
                .Concat(maybe => maybe
                    .Filter(x => x == 3)
                    .Match(
                        _ => Observable.Return(4),
                        () => Observable.Return(-1)))
                .ToArray()
                .ToTask();

            Assert.Equal(new[] { 1, 2, 3, 4 }, array);
        }

        [Fact]
        public async Task Observable_Concat_produces_correct_sequence_when_first_sequence_is_empty()
        {
            var array = await Observable.Empty<int>()
                .Concat(maybe => Observable.Return(!maybe.IsSome ? 1 : 2))
                .ToArray()
                .ToTask();

            Assert.Equal(new[] { 1 }, array);
        }

        [Fact]
        public async Task Observable_Concat_produces_correct_sequence_when_first_sequence_faults()
        {
            var ex = new Exception();

            var array = await Observable.Throw<int>(ex)
                .Concat(maybe => Observable.Return(1))
                .Materialize()
                .ToArray()
                .ToTask();

            Assert.Equal(1, array.Length);
            Assert.Equal(ex, array[0].Exception);
        }

        [Fact]
        public async Task RepeatWhileEmpty_produces_correct_values()
        {
            var count = 0;

            var array = await Observable
                .Defer(() =>
                {
                    count++;

                    if (count <= 2)
                        return Observable.Empty<int>();

                    if (count == 3)
                        return new[] { 1, 2, 3 }.ToObservable();

                    return new[] { 4, 5, 6 }.ToObservable();
                })
                .RepeatWhileEmpty()
                .ToArray()
                .ToTask();

            Assert.Equal(new[] { 1, 2, 3 }, array);
        }

        [Fact]
        public async Task RepeatWhileEmpty_propagates_exception_correctly()
        {
            var count = 0;

            var t = Observable
                .Defer(() =>
                {
                    count++;

                    if (count <= 2)
                        return Observable.Empty<int>();

                    return Observable.Throw<int>(new DivideByZeroException());
                })
                .RepeatWhileEmpty()
                .ToArray()
                .ToTask();

            t
                .Awaiting(_ => _)
                .Should()
                .ThrowExactly<DivideByZeroException>();

        }

        [Fact]
        public async Task TakeWhileInclusive_takes_one_additional_element()
        {
            var array = await new[] { 1, 2, 3, 4, 5, 6 }
                .ToObservable()
                .RepeatWhileEmpty()
                .TakeWhileInclusive(x => x < 3)
                .ToArray()
                .ToTask();

            Assert.Equal(new[] { 1, 2, 3 }, array);
        }

        [Fact]
        public async Task TakeWhileInclusive_has_correct_indizes()
        {
            var array = await new[] { 0, 1, 2, 3, 4, 4, 5 }
                .ToObservable()
                .RepeatWhileEmpty()
                .TakeWhileInclusive((x, i) => x == i)
                .ToArray()
                .ToTask();

            Assert.Equal(new[] { 0, 1, 2, 3, 4, 4 }, array);
        }

        [Fact]
        public async Task Where_with_async_predicate_does_the_job()
        {
            var source = new[]{ 8, 2, 6, 5, 0, 1, 9, 7, 3, 4}.ToObservable();

            var filtered = await source
                .Where(
                    async (x, ct) =>
                    {
                        await Task.Delay(x * 10, ct);
                        return x % 2 == 0;
                    })
                .ToArray()
                .ToTask();

            filtered
                .Should()
                .BeEquivalentTo(new[] { 2, 4, 6, 8, 0 });
        }

        [Fact]
        public void Debounce_lets_first_value_pass()
        {
            var testScheduler = new TestScheduler();

            var xs = testScheduler.CreateHotObservable<int>(
                new Recorded<Notification<int>>(2, Notification.CreateOnNext(1)),
                new Recorded<Notification<int>>(3, Notification.CreateOnCompleted<int>()));

            var latestValue = 0;

            xs
                .Debounce(TimeSpan.FromTicks(200), false, testScheduler)
                .Subscribe(value => latestValue = value);

            testScheduler.AdvanceBy(1);
            Assert.Equal(0, latestValue);

            testScheduler.AdvanceBy(1);
            Assert.Equal(1, latestValue);
        }

        [Fact]
        public void Debounce_blocks_value_within_debounce_interval_does_not_emit_value_after_debounce_interval_if_configured()
        {
            var testScheduler = new TestScheduler();

            var xs = testScheduler.CreateHotObservable<int>(
                new Recorded<Notification<int>>(2, Notification.CreateOnNext(1)),
                new Recorded<Notification<int>>(3, Notification.CreateOnNext(2)),
                new Recorded<Notification<int>>(4, Notification.CreateOnCompleted<int>()));

            var latestValue = 0;

            xs
                .Debounce(TimeSpan.FromTicks(200), false, testScheduler)
                .Subscribe(value => latestValue = value);

            testScheduler.AdvanceBy(1);
            Assert.Equal(0, latestValue);

            testScheduler.AdvanceBy(1);
            Assert.Equal(1, latestValue);

            testScheduler.AdvanceBy(200);
            Assert.Equal(1, latestValue);
        }

        [Fact]
        public void Debounce_blocks_value_within_debounce_interval_and_emits_value_after_debounce_interval_if_configured()
        {
            var testScheduler = new TestScheduler();

            var xs = testScheduler.CreateHotObservable<int>(
                new Recorded<Notification<int>>(2, Notification.CreateOnNext(1)),
                new Recorded<Notification<int>>(3, Notification.CreateOnNext(2)),
                new Recorded<Notification<int>>(4, Notification.CreateOnNext(3)),
                new Recorded<Notification<int>>(400, Notification.CreateOnCompleted<int>()));

            var latestValue = 0;

            xs
                .Debounce(TimeSpan.FromTicks(200), true, testScheduler)
                .Subscribe(value => latestValue = value);

            testScheduler.AdvanceBy(1);
            Assert.Equal(0, latestValue);

            testScheduler.AdvanceBy(1);
            Assert.Equal(1, latestValue);

            testScheduler.AdvanceBy(200);
            Assert.Equal(3, latestValue);
        }

        [Fact]
        public void Debounce_blocks_value_within_debounce_interval_and_emits_nothing_if_completed_meanwhile()
        {
            var testScheduler = new TestScheduler();

            var xs = testScheduler.CreateHotObservable<int>(
                new Recorded<Notification<int>>(2, Notification.CreateOnNext(1)),
                new Recorded<Notification<int>>(3, Notification.CreateOnNext(2)),
                new Recorded<Notification<int>>(4, Notification.CreateOnNext(3)),
                new Recorded<Notification<int>>(5, Notification.CreateOnCompleted<int>()));

            var latestValue = 0;

            xs
                .Debounce(TimeSpan.FromTicks(200), true, testScheduler)
                .Subscribe(value => latestValue = value);

            testScheduler.AdvanceBy(1);
            Assert.Equal(0, latestValue);

            testScheduler.AdvanceBy(1);
            Assert.Equal(1, latestValue);

            testScheduler.AdvanceBy(200);
            Assert.Equal(1, latestValue);
        }
    }
}
