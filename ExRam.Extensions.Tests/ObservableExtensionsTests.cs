using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Disposables;
using Moq;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class ObservableExtensionsTests
    {
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
    }
}
