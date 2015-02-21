// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class AsyncEnumerable_Materialize_Test
    {
        #region AsyncEnumerable_Materialize_handles_OnNext_correctly
        [TestMethod]
        public async Task AsyncEnumerable_Materialize_handles_OnNext_correctly()
        {
            var notifications = await AsyncEnumerable.Range(0, 100)
                .Materialize()
                .Take(10)
                .ToArray();

            Assert.AreEqual(10, notifications.Length);
            Assert.IsTrue(notifications.All(x => x.Kind == NotificationKind.OnNext));

            for(var i = 0; i < notifications.Length; i++)
            {
                Assert.AreEqual(i, notifications[i].Value);
            }
        }
        #endregion

        #region AsyncEnumerable_Materialize_handles_OnCompleted_correctly
        [TestMethod]
        public async Task AsyncEnumerable_Materialize_handles_OnCompleted_correctly()
        {
            var notifications = await AsyncEnumerable.Range(0, 100)
                .Take(10)
                .Materialize()
                .ToArray();

            Assert.AreEqual(11, notifications.Length);

            for (var i = 0; i < notifications.Length - 1; i++)
            {
                Assert.AreEqual(NotificationKind.OnNext, notifications[i].Kind);
                Assert.AreEqual(i, notifications[i].Value);
            }

            var lastNotificaton = notifications.Last();

            Assert.AreEqual(NotificationKind.OnCompleted, lastNotificaton.Kind);
        }
        #endregion

        #region AsyncEnumerable_Materialize_handles_OnError_correctly
        [TestMethod]
        public async Task AsyncEnumerable_Materialize_handles_OnError_correctly()
        {
            var ex = new DivideByZeroException();
            var notifications = await AsyncEnumerable.Range(0, 100)
                .Take(10)
                .Concat(AsyncEnumerable.Throw<int>(ex))
                .Materialize()
                .ToArray();

            Assert.AreEqual(11, notifications.Length);

            for (var i = 0; i < notifications.Length - 1; i++)
            {
                Assert.AreEqual(NotificationKind.OnNext, notifications[i].Kind);
                Assert.AreEqual(i, notifications[i].Value);
            }

            var lastNotificaton = notifications.Last();

            Assert.AreEqual(NotificationKind.OnError, lastNotificaton.Kind);
            Assert.AreEqual(ex, lastNotificaton.Exception);
        }
        #endregion
    }
}
