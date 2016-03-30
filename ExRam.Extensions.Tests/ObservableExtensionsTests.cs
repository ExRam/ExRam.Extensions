using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using ObservableExtensions = System.Reactive.Linq.ObservableExtensions;

namespace ExRam.Extensions.Tests
{
    public class ObservableExtensionsTests
    {
        #region SubscribeTotallyAtMostTest
        [Fact]
        public void SubscribeTotallyAtMostTest()
        {
            var observableMock = new Mock<IObservable<object>>();
            observableMock.Setup(x => x.Subscribe(It.IsAny<IObserver<object>>())).Returns(Disposable.Empty);

            var subscribeAtMostObservable = observableMock.Object.SubscribeTotallyAtMost(3, Observable.Empty<object>());

            for (var i = 0; i < 6; i++)
            {
                subscribeAtMostObservable.Subscribe();
            }

            observableMock.Verify(x => x.Subscribe(It.IsAny<IObserver<object>>()), Times.Exactly(3));
        }
        #endregion

        #region Observable_Current_blocks_if_no_current_element_is_present
        [Fact]
        public async Task Observable_Current_blocks_if_no_current_element_is_present()
        {
            var subject = new Subject<int>();
            var asyncEnumerable = subject.Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                var moveNextTask = asyncEnumerator.MoveNext(CancellationToken.None);
                await Task.Delay(TimeSpan.FromMilliseconds(200));

                Assert.False(moveNextTask.IsCompleted);
                Assert.False(moveNextTask.IsCanceled);

                subject.OnNext(1);

                Assert.True(await asyncEnumerator.MoveNext(CancellationToken.None));
                Assert.Equal(1, asyncEnumerator.Current);
            }
        }
        #endregion

        #region Observable_Current_returns_latest_element
        [Fact]
        public async Task Observable_Current_returns_latest_element()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                subject.OnNext(2);
                subject.OnNext(3);
                subject.OnNext(4);

                Assert.True(await asyncEnumerator.MoveNext(CancellationToken.None));
                Assert.Equal(4, asyncEnumerator.Current);
            }
        }
        #endregion

        #region Observable_Current_returns_elements_repeatedly
        [Fact]
        public async Task Observable_Current_returns_elements_repeatedly()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                for (var i = 0; i < 10; i++)
                {
                    Assert.True(await asyncEnumerator.MoveNext(CancellationToken.None));
                    Assert.Equal(1, asyncEnumerator.Current);
                }

                subject.OnNext(2);

                for (var i = 0; i < 10; i++)
                {
                    Assert.True(await asyncEnumerator.MoveNext(CancellationToken.None));
                    Assert.Equal(2, asyncEnumerator.Current);
                }
            }
        }
        #endregion

        #region Observable_Current_propagates_exception
        [Fact]
        public async Task Observable_Current_propagates_exception()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                subject.OnNext(1);
                subject.OnNext(2);
                subject.OnNext(3);

                subject.OnError(new InvalidOperationException());

                asyncEnumerator
                    .Awaiting(_ => _.MoveNext(CancellationToken.None))
                    .ShouldThrowExactly<AggregateException>()
                    .Where(ex => ex.GetBaseException() is InvalidOperationException);
            }
        }
        #endregion

        #region Observable_Current_completes_on_source_completion
        [Fact]
        public async Task Observable_Current_completes_on_source_completion()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                subject.OnNext(1);
                subject.OnNext(2);
                subject.OnNext(3);

                subject.OnCompleted();

                Assert.False(await asyncEnumerator.MoveNext(CancellationToken.None));
            }
        }
        #endregion

        #region Observable_Current_enumerator_disposal_cancels_moveNext
        [Fact]
        public async Task Observable_Current_enumerator_disposal_cancels_moveNext()
        {
            var asyncEnumerable = Observable.Never<int>().Current();

            Task moveNextTask;

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                moveNextTask = asyncEnumerator.MoveNext(CancellationToken.None);
            }

            moveNextTask
                .Awaiting(_ => _)
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region Observable_Current_MoveNext_cancellation_is_effective
        [Fact]
        public async Task Observable_Current_MoveNext_cancellation_is_effective()
        {
            var asyncEnumerable = Observable.Never<int>().Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                var cts = new CancellationTokenSource();
                var moveNextTask = asyncEnumerator.MoveNext(cts.Token);

                await Task.Delay(TimeSpan.FromMilliseconds(200));
                Assert.False(moveNextTask.IsCanceled);
                Assert.False(moveNextTask.IsCompleted);

                cts.Cancel();

                moveNextTask
                    .Awaiting(_ => _)
                    .ShouldThrowExactly<TaskCanceledException>();
            }
        }
        #endregion

        #region Observable_Concat_produces_correct_sequence_when_first_sequence_has_values
        [Fact]
        public async Task Observable_Concat_produces_correct_sequence_when_first_sequence_has_values()
        {
            var array = await new[] { 1, 2, 3 }
                .ToObservable()
                .Concat(maybe => maybe.GetValue() == 3 ? Observable.Return(4) : Observable.Return(-1))
                .ToArray()
                .ToTask();

            Assert.Equal(new[] { 1, 2, 3, 4 }, array);
        }
        #endregion

        #region Observable_Concat_produces_correct_sequence_when_first_sequence_is_empty
        [Fact]
        public async Task Observable_Concat_produces_correct_sequence_when_first_sequence_is_empty()
        {
            var array = await Observable.Empty<int>()
                .Concat(maybe => Observable.Return(!maybe.IsSome ? 1 : 2))
                .ToArray()
                .ToTask();

            Assert.Equal(new[] { 1 }, array);
        }
        #endregion

        #region Observable_Concat_produces_correct_sequence_when_first_sequence_faults
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
        #endregion

        #region RepeatWhileEmpty_produces_correct_values
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
        #endregion

        #region RepeatWhileEmpty_propagates_exception_correctly
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

                    return Observable.Throw<int>(new InvalidOperationException());
                })
                .RepeatWhileEmpty()
                .ToArray()
                .ToTask();

            t
                .Awaiting(_ => _)
                .ShouldThrowExactly<InvalidOperationException>();

        }
        #endregion

        #region TakeWhileInclusive_takes_one_additional_element
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
        #endregion

        #region TakeWhileInclusive_has_correct_indizes
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
        #endregion

        #region Where_with_async_predicate_does_the_job
        [Fact]
        public async Task Where_with_async_predicate_does_the_job()
        {
            var source = new[]{ 8, 2, 6, 5, 0, 1, 9, 7, 3, 4}.ToObservable();

            var filtered = await source
                .Where(async x =>
                {
                    await Task.Delay(x * 10);
                    return x % 2 == 0;
                })
                .ToArray()
                .ToTask();

            filtered
                .Should()
                .BeEquivalentTo(new[] { 2, 4, 6, 8, 0 });
        }
        #endregion

        #region LazyRefCount_connects
        [Fact]
        public void LazyRefCount_connects()
        {
            var connectionMock = new Mock<IDisposable>();
            var connectableObservableMock = new Mock<IConnectableObservable<Unit>>();

            connectableObservableMock
                .Setup(x => x.Connect())
                .Returns(connectionMock.Object);

            connectableObservableMock
                .Setup(x => x.Subscribe(It.IsAny<IObserver<Unit>>()))
                .Returns(Disposable.Empty);

            var subscription = connectableObservableMock
                .Object
                .LazyRefCount(TimeSpan.FromSeconds(10), Scheduler.Default)
                .Subscribe();

            using (subscription)
            {
                connectableObservableMock
                    .Verify(x => x.Connect(), Times.Once());

                connectionMock
                    .Verify(x => x.Dispose(), Times.Never());
            }
        }
        #endregion

        #region LazyRefCount_does_not_disconnect_within_lazy_time
        [Fact(Skip = "Rx-Testing cannot be used at this time.")]
        public async Task LazyRefCount_does_not_disconnect_within_lazy_time()
        {
            /*var testScheduler = new TestScheduler();

            var connectionMock = new Mock<IDisposable>();
            var connectableObservableMock = new Mock<IConnectableObservable<Unit>>();

            connectableObservableMock
                .Setup(x => x.Connect())
                .Returns(connectionMock.Object);

            connectableObservableMock
                .Setup(x => x.Subscribe(It.IsAny<IObserver<Unit>>()))
                .Returns(Disposable.Empty);

            var subscription = connectableObservableMock
                .Object
                .LazyRefCount(TimeSpan.FromTicks(2), testScheduler)
                .Subscribe();

            using (subscription)
            {
                connectableObservableMock
                    .Verify(x => x.Connect(), Times.Once());
            }

            connectionMock
                .Verify(x => x.Dispose(), Times.Never());

            testScheduler.AdvanceBy(2);

            connectionMock
                .Verify(x => x.Dispose(), Times.Once());*/
        }
        #endregion

        #region LazyRefCount_does_not_disconnect_after_lazy_time_when_new_connection_is_made
        [Fact(Skip = "Rx-Testing cannot be used at this time.")]
        public async Task LazyRefCount_does_not_disconnect_after_lazy_time_when_new_connection_is_made()
        {
            //var testScheduler = new TestScheduler();

            //var connectionMock = new Mock<IDisposable>();
            //var connectableObservableMock = new Mock<IConnectableObservable<Unit>>();

            //connectableObservableMock
            //    .Setup(x => x.Connect())
            //    .Returns(connectionMock.Object);

            //connectableObservableMock
            //    .Setup(x => x.Subscribe(It.IsAny<IObserver<Unit>>()))
            //    .Returns(Disposable.Empty);

            //var refCount = connectableObservableMock
            //    .Object
            //    .LazyRefCount(TimeSpan.FromTicks(20), testScheduler);

            //using (refCount.Subscribe())
            //{
            //    connectableObservableMock
            //        .Verify(x => x.Connect(), Times.Once());
            //}

            //connectionMock
            //    .Verify(x => x.Dispose(), Times.Never());

            //testScheduler.AdvanceBy(19);

            //connectionMock
            //    .Verify(x => x.Dispose(), Times.Never());

            //using (refCount.Subscribe())
            //{
            //    testScheduler.AdvanceBy(2);

            //    connectionMock
            //        .Verify(x => x.Dispose(), Times.Never());
            //}
        }
        #endregion

        #region LazyRefCount_disconnects_after_all_when_second_subscription_is_disposed
        [Fact(Skip = "Rx-Testing cannot be used at this time.")]
        public async Task LazyRefCount_disconnects_after_all_when_second_subscription_is_disposed()
        {
            //var testScheduler = new TestScheduler();

            //var connectionMock = new Mock<IDisposable>();
            //var connectableObservableMock = new Mock<IConnectableObservable<Unit>>();

            //connectableObservableMock
            //    .Setup(x => x.Connect())
            //    .Returns(connectionMock.Object);

            //connectableObservableMock
            //    .Setup(x => x.Subscribe(It.IsAny<IObserver<Unit>>()))
            //    .Returns(Disposable.Empty);

            //var refCount = connectableObservableMock
            //    .Object
            //    .LazyRefCount(TimeSpan.FromTicks(20), testScheduler);

            //refCount.Subscribe().Dispose();

            //testScheduler.AdvanceBy(19);

            //using (refCount.Subscribe())
            //{
            //    testScheduler.AdvanceBy(2);
            //}

            //testScheduler.AdvanceBy(20);

            //connectionMock
            //    .Verify(x => x.Dispose(), Times.Once());
        }
        #endregion

        #region LazyRefCount_does_not_observe_values_after_unsubscription()
        [Fact(Skip = "Rx-Testing cannot be used at this time.")]
        public async Task LazyRefCount_does_not_observe_values_after_unsubscription()
        {
            //var value = 0;
            //var subject = new Subject<int>();

            //var subscription = subject
            //    .Publish()
            //    .LazyRefCount(TimeSpan.FromSeconds(2), Scheduler.Default)
            //    .Subscribe(Observer.Create<int>(x =>
            //    {
            //        value = x;
            //    }));

            //using (subscription)
            //{
            //    subject.OnNext(1);
            //    Assert.Equal(1, value);
            //}

            //subject.OnNext(2);
            //Assert.Equal(1, value);
        }
        #endregion

        [Fact(Skip = "Rx-Testing cannot be used at this time.")]
        public void Debounce_lets_first_value_pass()
        {
            //var testScheduler = new TestScheduler();

            //var xs = testScheduler.CreateHotObservable<int>(
            //    new Recorded<Notification<int>>(2, Notification.CreateOnNext(1)),
            //    new Recorded<Notification<int>>(3, Notification.CreateOnCompleted<int>()));

            //var latestValue = 0;

            //xs
            //    .Debounce(TimeSpan.FromTicks(200), false, testScheduler)
            //    .Subscribe(value => latestValue = value);

            //testScheduler.AdvanceBy(1);
            //Assert.Equal(0, latestValue);

            //testScheduler.AdvanceBy(1);
            //Assert.Equal(1, latestValue);
        }

        [Fact(Skip = "Rx-Testing cannot be used at this time.")]
        public void Debounce_blocks_value_within_debounce_interval_does_not_emit_value_after_debounce_interval_if_configured()
        {
            //var testScheduler = new TestScheduler();

            //var xs = testScheduler.CreateHotObservable<int>(
            //    new Recorded<Notification<int>>(2, Notification.CreateOnNext(1)),
            //    new Recorded<Notification<int>>(3, Notification.CreateOnNext(2)),
            //    new Recorded<Notification<int>>(4, Notification.CreateOnCompleted<int>()));

            //var latestValue = 0;

            //xs
            //    .Debounce(TimeSpan.FromTicks(200), false, testScheduler)
            //    .Subscribe(value => latestValue = value);

            //testScheduler.AdvanceBy(1);
            //Assert.Equal(0, latestValue);

            //testScheduler.AdvanceBy(1);
            //Assert.Equal(1, latestValue);

            //testScheduler.AdvanceBy(200);
            //Assert.Equal(1, latestValue);
        }

        [Fact(Skip = "Rx-Testing cannot be used at this time.")]
        public void Debounce_blocks_value_within_debounce_interval_and_emits_value_after_debounce_interval_if_configured()
        {
            //var testScheduler = new TestScheduler();

            //var xs = testScheduler.CreateHotObservable<int>(
            //    new Recorded<Notification<int>>(2, Notification.CreateOnNext(1)),
            //    new Recorded<Notification<int>>(3, Notification.CreateOnNext(2)),
            //    new Recorded<Notification<int>>(4, Notification.CreateOnNext(3)),
            //    new Recorded<Notification<int>>(400, Notification.CreateOnCompleted<int>()));

            //var latestValue = 0;

            //xs
            //    .Debounce(TimeSpan.FromTicks(200), true, testScheduler)
            //    .Subscribe(value => latestValue = value);

            //testScheduler.AdvanceBy(1);
            //Assert.Equal(0, latestValue);

            //testScheduler.AdvanceBy(1);
            //Assert.Equal(1, latestValue);

            //testScheduler.AdvanceBy(200);
            //Assert.Equal(3, latestValue);
        }

        [Fact(Skip = "Rx-Testing cannot be used at this time.")]
        public void Debounce_blocks_value_within_debounce_interval_and_emits_nothing_if_completed_meanwhile()
        {
            //var testScheduler = new TestScheduler();

            //var xs = testScheduler.CreateHotObservable<int>(
            //    new Recorded<Notification<int>>(2, Notification.CreateOnNext(1)),
            //    new Recorded<Notification<int>>(3, Notification.CreateOnNext(2)),
            //    new Recorded<Notification<int>>(4, Notification.CreateOnNext(3)),
            //    new Recorded<Notification<int>>(5, Notification.CreateOnCompleted<int>()));

            //var latestValue = 0;

            //xs
            //    .Debounce(TimeSpan.FromTicks(200), true, testScheduler)
            //    .Subscribe(value => latestValue = value);

            //testScheduler.AdvanceBy(1);
            //Assert.Equal(0, latestValue);

            //testScheduler.AdvanceBy(1);
            //Assert.Equal(1, latestValue);

            //testScheduler.AdvanceBy(200);
            //Assert.Equal(1, latestValue);
        }
    }
}
