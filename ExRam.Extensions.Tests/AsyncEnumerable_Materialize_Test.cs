// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class AsyncEnumerable_Materialize_Test
    {
        [Fact]
        public async Task AsyncEnumerable_Materialize_handles_OnNext_correctly()
        {
            var notifications = await AsyncEnumerable.Range(0, 100)
                .Materialize()
                .Take(10)
                .ToArray();

            Assert.Equal(10, notifications.Length);
            Assert.True(notifications.All(x => x.Kind == NotificationKind.OnNext));

            for(var i = 0; i < notifications.Length; i++)
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
    }
}
