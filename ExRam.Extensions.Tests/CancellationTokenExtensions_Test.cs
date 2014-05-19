﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Extensions.Tests
{
    [TestClass]
    public class CancellationTokenExtensions_Test
    {
        [TestMethod]
        public async Task ToObservable_produces_value_on_cancellation()
        {
            var cts = new CancellationTokenSource();
            var unitTask = cts.Token.ToObservable().FirstAsync().ToTask();

            Assert.IsFalse(unitTask.IsCompleted);

            cts.Cancel();

            await unitTask;
        }
    }
}