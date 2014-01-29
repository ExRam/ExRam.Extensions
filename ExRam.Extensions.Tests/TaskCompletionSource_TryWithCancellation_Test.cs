// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace ExRam.Framework.Tests
{
    [TestClass]
    public class TaskCompletionSource_TryWithCancellation_Test
    {
        #region WithCancellation_cancels_uncompleted_TaskCompletionSource1
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task WithCancellation_cancels_uncompleted_TaskCompletionSource1()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<object>();
            
            tcs = tcs.WithCancellation(cts.Token);
            cts.Cancel();

            await tcs.Task;
        }
        #endregion

        #region WithCancellation_cancels_uncompleted_TaskCompletionSource2
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task WithCancellation_cancels_uncompleted_TaskCompletionSource2()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<object>();

            cts.Cancel();
            tcs = tcs.WithCancellation(cts.Token);

            await tcs.Task;
        }
        #endregion

        #region WithCancellation_does_not_cancel_completed_TaskCompletionSource1
        [TestMethod]
        public async Task WithCancellation_does_not_cancel_completed_TaskCompletionSource1()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            
            tcs = tcs.WithCancellation(cts.Token);
            cts.Cancel();

            await tcs.Task;
        }
        #endregion

        #region WithCancellation_does_not_cancel_completed_TaskCompletionSource2
        [TestMethod]
        public async Task WithCancellation_does_not_cancel_completed_TaskCompletionSource2()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);

            cts.Cancel();
            tcs = tcs.WithCancellation(cts.Token);

            await tcs.Task;
        }
        #endregion

        #region WithCancellation_cancels_uncompleted_TaskCompletionSource_of_Int_1
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task WithCancellation_cancels_uncompleted_TaskCompletionSource_of_Int_1()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<int>();

            tcs = tcs.WithCancellation(cts.Token);
            cts.Cancel();

            await tcs.Task;
        }
        #endregion

        #region WithCancellation_cancels_uncompleted_TaskCompletionSource_of_Int_2
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task WithCancellation_cancels_uncompleted_TaskCompletionSource_of_Int_2()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<int>();

            cts.Cancel();
            tcs = tcs.WithCancellation(cts.Token);

            await tcs.Task;
        }
        #endregion

        #region WithCancellation_does_not_cancel_completed_TaskCompletionSource_of_int_1
        [TestMethod]
        public async Task WithCancellation_does_not_cancel_completed_TaskCompletionSource_of_int_1()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(36);

            tcs = tcs.WithCancellation(cts.Token);
            cts.Cancel();

            Assert.AreEqual(36, await tcs.Task);
        }
        #endregion

        #region WithCancellation_does_not_cancel_completed_TaskCompletionSource_of_int_2
        [TestMethod]
        public async Task WithCancellation_does_not_cancel_completed_TaskCompletionSource_of_int_2()
        {
            var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(36);

            cts.Cancel();
            tcs = tcs.WithCancellation(cts.Token);

            Assert.AreEqual(36, await tcs.Task);
        }
        #endregion
    }
}