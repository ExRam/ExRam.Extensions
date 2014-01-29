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
    public class Task_TryWithCancellation_Test
    {
        #region TryWithCancellation_asynchronously_returns_false_if_cancelled_after_call
        [TestMethod]
        public async Task TryWithCancellation_asynchronously_returns_false_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.AreEqual(false, await cancellationTask);
        }
        #endregion

        #region TryWithCancellation_asynchronously_returns_false_if_cancelled_before_call
        [TestMethod]
        public async Task TryWithCancellation_asynchronously_returns_false_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.AreEqual(false, await cancellationTask);
        }
        #endregion

        #region TryWithCancellation_asynchronously_returns_true_if_not_cancelled
        [TestMethod]
        public void TryWithCancellation_asynchronously_returns_true_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() => Thread.Sleep(100));
            var cts = new CancellationTokenSource();

            Assert.AreEqual(true, task.TryWithCancellation(cts.Token).Result);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_after_call
        [TestMethod]
        public async Task TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.IsFalse((await cancellationTask).HasValue);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_before_call
        [TestMethod]
        public async Task TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.IsFalse((await cancellationTask).HasValue);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_not_cancelled
        [TestMethod]
        [Description("Sind die Felder im MaybeCancelled-Objekt aus dem Rückgabe-Task von WithCancellation korrekt, wenn das CancellationToken nicht gecancellt wird, und der Task ganz normal 36 zurückgibt ?")]
        public async Task TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                return 36;
            });

            var cts = new CancellationTokenSource();

            var maybeInt = await task.TryWithCancellation(cts.Token);

            Assert.IsTrue(maybeInt.HasValue);
            Assert.AreEqual(36, maybeInt.Value);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_after_call
        [TestMethod]
        public async Task TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Maybe<int>>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.IsFalse((await cancellationTask).HasValue);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_before_call
        [TestMethod]
        public async Task TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Maybe<int>>();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.IsFalse((await cancellationTask).HasValue);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_not_cancelled
        [TestMethod]
        public async Task TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                return (Maybe<int>)36;
            });

            var cts = new CancellationTokenSource();

            var maybeInt = await task.TryWithCancellation(cts.Token);

            Assert.IsTrue(maybeInt.HasValue);
            Assert.AreEqual(36, maybeInt.Value);
        }
        #endregion

        #region TryWithCancellation_throws_if_called_on_cancelled_task
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task TryWithCancellation_throws_if_called_on_cancelled_task()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetCanceled();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            await cancellationTask;
        }
        #endregion

        #region Exceptions_Are_Propagated_Through_TryWithCancellation_Of_Task
        [TestMethod]
        public async Task Exceptions_Are_Propagated_Through_TryWithCancellation_Of_Task()
        {
            var ex = new ApplicationException();

            Func<Task> faultingTaskFunc = async () =>
            {
                throw ex;
            };

            try
            {
                await faultingTaskFunc().TryWithCancellation(CancellationToken.None);
                Assert.Fail();
            }
            catch (ApplicationException ex2)
            {
                Assert.AreEqual(ex, ex2);
            }
        }
        #endregion

        #region Exceptions_Are_Propagated_Through_TryWithCancellation_With_Task_Of_Int
        [TestMethod]
        public async Task Exceptions_Are_Propagated_Through_TryWithCancellation_With_Task_Of_Int()
        {
            var ex = new ApplicationException();

            Func<Task<int>> faultingTaskFunc = async () =>
            {
                throw ex;
            };

            try
            {
                await faultingTaskFunc().TryWithCancellation(CancellationToken.None);
                Assert.Fail();
            }
            catch (ApplicationException ex2)
            {
                Assert.AreEqual(ex, ex2);
            }
        }
        #endregion
    }
}