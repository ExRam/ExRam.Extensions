using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class ObservableExtensionsTests
    {
        [TestMethod]
        public void SubscribeAtMostTest()
        {
            var obs = Observable.Return(36)
                .SubscribeTotallyAtMost(3);

            for (var i = 0; i < 4; i++)
            {
                var called = false;

                obs.Subscribe((v) => called = true);
                
                if (i < 3)
                    Assert.IsTrue(called);
                else
                    Assert.IsFalse(called);
            }
        }
    }
}
