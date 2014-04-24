using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

                await asyncEnumerator.MoveNext(CancellationToken.None);
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
    }
}
