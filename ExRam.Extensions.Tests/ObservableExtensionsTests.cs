using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObservableExtensions = System.Reactive.Linq.ObservableExtensions;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class ObservableExtensionsTests
    {
        #region SubscribeTotallyAtMostTest
        [TestMethod]
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

        #region ConnectTotallyAtMostTest
        [TestMethod]
        public void ConnectTotallyAtMostTest()
        {
            var observableMock = new Mock<IConnectableObservable<object>>();
            observableMock.Setup(x => x.Connect()).Returns(Disposable.Empty);

            var connectAtMostObservable = observableMock.Object.ConnectTotallyAtMost(3);

            for (var i = 0; i < 6; i++)
            {
                connectAtMostObservable.Connect();
            }

            observableMock.Verify(x => x.Connect(), Times.Exactly(3));
        }
        #endregion

        #region DisconnectTotallyAtMostTest
        [TestMethod]
        public void DisconnectTotallyAtMostTest()
        {
            var disposableMock = new Mock<IDisposable>();
            var observableMock = new Mock<IConnectableObservable<object>>();
            observableMock.Setup(x => x.Connect()).Returns(disposableMock.Object);

            var connectAtMostObservable = observableMock.Object.DisconnectTotallyAtMost(3);

            for (var i = 0; i < 6; i++)
            {
                connectAtMostObservable.Connect().Dispose();
            }

            observableMock.Verify(x => x.Connect(), Times.Exactly(6));
            disposableMock.Verify(x => x.Dispose(), Times.Exactly(3));
        }
        #endregion

        #region Observable_Current_blocks_if_no_current_element_is_present
        [TestMethod]
        public async Task Observable_Current_blocks_if_no_current_element_is_present()
        {
            var subject = new Subject<int>();
            var asyncEnumerable = subject.Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                var moveNextTask = asyncEnumerator.MoveNext(CancellationToken.None);
                await Task.Delay(TimeSpan.FromMilliseconds(200));

                Assert.IsFalse(moveNextTask.IsCompleted);
                Assert.IsFalse(moveNextTask.IsCanceled);

                subject.OnNext(1);

                Assert.IsTrue(await asyncEnumerator.MoveNext(CancellationToken.None));
                Assert.AreEqual(1, asyncEnumerator.Current);
            }
        }
        #endregion

        #region Observable_Current_returns_latest_element
        [TestMethod]
        public async Task Observable_Current_returns_latest_element()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                subject.OnNext(2);
                subject.OnNext(3);
                subject.OnNext(4);

                Assert.IsTrue(await asyncEnumerator.MoveNext(CancellationToken.None));
                Assert.AreEqual(4, asyncEnumerator.Current);
            }
        }
        #endregion

        #region Observable_Current_returns_elements_repeatedly
        [TestMethod]
        public async Task Observable_Current_returns_elements_repeatedly()
        {
            var subject = new BehaviorSubject<int>(1);
            var asyncEnumerable = subject.Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                for (var i = 0; i < 10; i++)
                {
                    Assert.IsTrue(await asyncEnumerator.MoveNext(CancellationToken.None));
                    Assert.AreEqual(1, asyncEnumerator.Current);
                }

                subject.OnNext(2);

                for (var i = 0; i < 10; i++)
                {
                    Assert.IsTrue(await asyncEnumerator.MoveNext(CancellationToken.None));
                    Assert.AreEqual(2, asyncEnumerator.Current);
                }
            }
        }
        #endregion

        #region Observable_Current_propagates_exception
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
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

                try
                {
                    await asyncEnumerator.MoveNext(CancellationToken.None);
                }
                catch(AggregateException ex)
                {
                    throw ex.GetBaseException();
                }

                Assert.Fail();
            }
        }
        #endregion

        #region Observable_Current_completes_on_source_completion
        [TestMethod]
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

                Assert.IsFalse(await asyncEnumerator.MoveNext(CancellationToken.None));
            }
        }
        #endregion

        #region Observable_Current_enumerator_disposal_cancels_moveNext
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Observable_Current_enumerator_disposal_cancels_moveNext()
        {
            var asyncEnumerable = Observable.Never<int>().Current();

            Task moveNextTask;

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                moveNextTask = asyncEnumerator.MoveNext(CancellationToken.None);
            }

            await moveNextTask;
        }
        #endregion

        #region Observable_Current_MoveNext_cancellation_is_effective
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Observable_Current_MoveNext_cancellation_is_effective()
        {
            var asyncEnumerable = Observable.Never<int>().Current();

            using (var asyncEnumerator = asyncEnumerable.GetEnumerator())
            {
                var cts = new CancellationTokenSource();
                var moveNextTask = asyncEnumerator.MoveNext(cts.Token);

                await Task.Delay(TimeSpan.FromMilliseconds(200));
                Assert.IsFalse(moveNextTask.IsCanceled);
                Assert.IsFalse(moveNextTask.IsCompleted);

                cts.Cancel();

                await moveNextTask;
            }
        }
        #endregion

        #region Observable_Concat_produces_correct_sequence_when_first_sequence_has_values
        [TestMethod]
        public async Task Observable_Concat_produces_correct_sequence_when_first_sequence_has_values()
        {
            var array = await new[] { 1, 2, 3 }
                .ToObservable()
                .Concat(maybe => maybe.Value == 3 ? Observable.Return(4) : Observable.Return(-1))
                .ToArray()
                .ToTask();

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, array);
        }
        #endregion

        #region Observable_Concat_produces_correct_sequence_when_first_sequence_is_empty
        [TestMethod]
        public async Task Observable_Concat_produces_correct_sequence_when_first_sequence_is_empty()
        {
            var array = await Observable.Empty<int>()
                .Concat(maybe => Observable.Return(!maybe.HasValue ? 1 : 2))
                .ToArray()
                .ToTask();

            CollectionAssert.AreEqual(new[] { 1 }, array);
        }
        #endregion

        #region Observable_Concat_produces_correct_sequence_when_first_sequence_faults
        [TestMethod]
        public async Task Observable_Concat_produces_correct_sequence_when_first_sequence_faults()
        {
            var ex = new Exception();

            var array = await Observable.Throw<int>(ex)
                .Concat(maybe => Observable.Return(1))
                .Materialize()
                .ToArray()
                .ToTask();

            Assert.AreEqual(1, array.Length);
            Assert.AreEqual(ex, array[0].Exception);
        }
        #endregion

        #region RepeatWhileEmpty_produces_correct_values
        [TestMethod]
        public async Task RepeatWhileEmpty_produces_correct_values()
        {
            var array = await ObservableExtensions
                .Morph(
                    Observable.Empty<int>(),
                    Observable.Empty<int>(),
                    new[] { 1, 2, 3 }.ToObservable(),
                    new[] { 4, 5, 6 }.ToObservable())
                .RepeatWhileEmpty()
                .ToArray()
                .ToTask();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, array);
        }
        #endregion

        #region RepeatWhileEmpty_propagates_exception_correctly
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task RepeatWhileEmpty_propagates_exception_correctly()
        {
            await ObservableExtensions
                .Morph(
                    Observable.Empty<int>(),
                    Observable.Empty<int>(),
                    Observable.Throw<int>(new InvalidOperationException()))
                .RepeatWhileEmpty()
                .ToArray()
                .ToTask();
        }
        #endregion

        #region TakeWhileInclusive_takes_one_additional_element
        [TestMethod]
        public async Task TakeWhileInclusive_takes_one_additional_element()
        {
            var array = await new[] { 1, 2, 3, 4, 5, 6 }
                .ToObservable()
                .TakeWhileInclusive(x => x < 3)
                .ToArray()
                .ToTask();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, array);
        }
        #endregion

        #region Where_with_async_predicate_does_the_job
        [TestMethod]
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

            CollectionAssert.AreEquivalent(new[] { 2, 4, 6, 8, 0 }, filtered);
        }
        #endregion


        #region LazyRefCount_connects
        [TestMethod]
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
        [TestMethod]
        public async Task LazyRefCount_does_not_disconnect_within_lazy_time()
        {
            var testScheduler = new TestScheduler();

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
                .Verify(x => x.Dispose(), Times.Once());
        }
        #endregion

        #region LazyRefCount_does_not_disconnect_after_lazy_time_when_new_connection_is_made
        [TestMethod]
        public async Task LazyRefCount_does_not_disconnect_after_lazy_time_when_new_connection_is_made()
        {
            var testScheduler = new TestScheduler();

            var connectionMock = new Mock<IDisposable>();
            var connectableObservableMock = new Mock<IConnectableObservable<Unit>>();

            connectableObservableMock
                .Setup(x => x.Connect())
                .Returns(connectionMock.Object);

            connectableObservableMock
                .Setup(x => x.Subscribe(It.IsAny<IObserver<Unit>>()))
                .Returns(Disposable.Empty);

            var refCount = connectableObservableMock
                .Object
                .LazyRefCount(TimeSpan.FromTicks(20), testScheduler);

            using (refCount.Subscribe())
            {
                connectableObservableMock
                    .Verify(x => x.Connect(), Times.Once());
            }

            connectionMock
                .Verify(x => x.Dispose(), Times.Never());

            testScheduler.AdvanceBy(19);

            connectionMock
                .Verify(x => x.Dispose(), Times.Never());

            using (refCount.Subscribe())
            {
                testScheduler.AdvanceBy(2);

                connectionMock
                    .Verify(x => x.Dispose(), Times.Never());
            }
        }
        #endregion

        #region LazyRefCount_disconnects_after_all_when_second_subscription_is_disposed
        [TestMethod]
        public async Task LazyRefCount_disconnects_after_all_when_second_subscription_is_disposed()
        {
            var testScheduler = new TestScheduler();

            var connectionMock = new Mock<IDisposable>();
            var connectableObservableMock = new Mock<IConnectableObservable<Unit>>();

            connectableObservableMock
                .Setup(x => x.Connect())
                .Returns(connectionMock.Object);

            connectableObservableMock
                .Setup(x => x.Subscribe(It.IsAny<IObserver<Unit>>()))
                .Returns(Disposable.Empty);

            var refCount = connectableObservableMock
                .Object
                .LazyRefCount(TimeSpan.FromTicks(20), testScheduler);

            refCount.Subscribe().Dispose();

            testScheduler.AdvanceBy(19);

            using (refCount.Subscribe())
            {
                testScheduler.AdvanceBy(2);
            }

            testScheduler.AdvanceBy(20);

            connectionMock
                .Verify(x => x.Dispose(), Times.Once());
        }
        #endregion

        #region LazyRefCount_does_not_observe_values_after_unsubscription()
        [TestMethod]
        public async Task LazyRefCount_does_not_observe_values_after_unsubscription()
        {
            var value = 0;
            var subject = new Subject<int>();

            var subscription = subject
                .Publish()
                .LazyRefCount(TimeSpan.FromSeconds(2), Scheduler.Default)
                .Subscribe(Observer.Create<int>(x =>
                {
                    value = x;
                }));

            using (subscription)
            {
                subject.OnNext(1);
                Assert.AreEqual(1, value);
            }

            subject.OnNext(2);
            Assert.AreEqual(1, value);
        }
        #endregion
    }
}
