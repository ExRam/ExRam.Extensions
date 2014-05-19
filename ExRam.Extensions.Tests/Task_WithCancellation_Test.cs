// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Framework.Tests
{
    [TestClass]
    public class Task_WithCancellation_Test
    {
        #region WithCancellation_throws_if_cancelled_after_call
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task WithCancellation_throws_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted();
            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cts.Cancel();

            await cancellationTask;
        }
        #endregion

        #region WithCancellation_throws_if_cancelled_before_call
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task WithCancellation_throws_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted();

            cts.Cancel();

            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            await cancellationTask;
        }
        #endregion

        #region WithCancellation_succeeds_if_not_cancelled
        [TestMethod]
        public async Task WithCancellation_succeeds_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() => Thread.Sleep(100));
            var cts = new CancellationTokenSource();

            await task.WithCancellation(cts.Token);
        }
        #endregion

        #region WithCancellation_with_TaskOfInt_throws_if_cancelled_after_call
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task WithCancellation_with_TaskOfInt_throws_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();
            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cts.Cancel();

            await cancellationTask;
        }
        #endregion

        #region WithCancellation_with_TaskOfInt_throws_if_cancelled_before_call
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task WithCancellation_with_TaskOfInt_throws_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();

            cts.Cancel();

            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            await cancellationTask;
        }
        #endregion

        #region WithCancellation_with_TaskOfInt_succeeds_if_not_cancelled
        [TestMethod]
        public async Task WithCancellation_with_TaskOfInt_succeeds_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                return 36;
            });

            var cts = new CancellationTokenSource();

            Assert.AreEqual(36, await task.WithCancellation(cts.Token));
        }
        #endregion

        #region Exceptions_Are_Propagated_Through_WithCancellation_Of_Task
        [TestMethod]
        public async Task Exceptions_Are_Propagated_Through_WithCancellation_Of_Task()
        {
            var ex = new ApplicationException();

            Func<Task> faultingTaskFunc = async () =>
            {
                throw ex;
            };

            try
            {
                await faultingTaskFunc().WithCancellation(CancellationToken.None);
                Assert.Fail();
            }
            catch (ApplicationException ex2)
            {
                Assert.AreEqual(ex, ex2);
            }
        }
        #endregion

        #region Exceptions_Are_Propagated_Through_WithCancellation_With_Task_Of_Int
        [TestMethod]
        public async Task Exceptions_Are_Propagated_Through_WithCancellation_With_Task_Of_Int()
        {
            var ex = new ApplicationException();

            Func<Task<int>> faultingTaskFunc = async () =>
            {
                throw ex;
            };

            try
            {
                await faultingTaskFunc().WithCancellation(CancellationToken.None);
                Assert.Fail();
            }
            catch (ApplicationException ex2)
            {
                Assert.AreEqual(ex, ex2);
            }
        }
        #endregion

        #region Exceptions_Are_Propagated_Through_Nested_TryWithCancellation_With_Task_Of_Int
        [TestMethod]
        public async Task Exceptions_Are_Propagated_Through_Nested_TryWithCancellation_With_Task_Of_Int()
        {
            var ex = new ApplicationException();

            Func<Task<int>> faultingTaskFunc = async () =>
            {
                throw ex;
            };

            try
            {
                await faultingTaskFunc()
                    .TryWithCancellation(CancellationToken.None)
                    .TryWithCancellation(CancellationToken.None);

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