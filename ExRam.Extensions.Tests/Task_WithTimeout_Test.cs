// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExRam.Framework.Tests
{
    [TestClass]
    public class Task_WithTimeout_Test
    {
        #region Completed_Task_WithTimeout_Completes
        [TestMethod]
        public async Task Completed_Task_WithTimeout_Completes()
        {
            var completedTask = Task.Factory.GetCompleted();
            await completedTask.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Completed_TaskOfInt_WithTimeout_Completes
        [TestMethod]
        public async Task Completed_TaskOfInt_WithTimeout_Completes()
        {
            var completedTask = Task.FromResult(36);
            Assert.AreEqual(36, await completedTask.WithTimeout(TimeSpan.FromMilliseconds(500)));
        }
        #endregion

        #region Faulted_Task_WithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Faulted_Task_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted(new InvalidOperationException());
            await completedTask.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Faulted_TaskOfInt_WithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Faulted_TaskOfInt_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<int>(new InvalidOperationException());
            Assert.AreEqual(36, await completedTask.WithTimeout(TimeSpan.FromMilliseconds(500)));
        }
        #endregion

        #region Canceled_Task_WithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Canceled_Task_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled();
            await completedTask.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Canceled_TaskOfInt_WithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Canceled_TaskOfInt_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<int>();
            Assert.AreEqual(36, await completedTask.WithTimeout(TimeSpan.FromMilliseconds(500)));
        }
        #endregion

        #region Uncompleted_Task_WithTimeout_faults_with_TimeoutException
        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task Uncompleted_Task_WithTimeout_faults_with_TimeoutException()
        {
            var uncompletedTask = Task.Factory.GetUncompleted();
            await uncompletedTask.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_TaskOfInt_WithTimeout_faults_with_TimeoutException
        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task Uncompleted_TaskOfInt_WithTimeout_faults_with_TimeoutException()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<int>();
            await uncompletedTask.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_Task_WithTimeout_can_complete_afterwards
        [TestMethod]
        public async Task Uncompleted_Task_WithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            try
            {
                await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
                Assert.Fail();
            }
            catch (TimeoutException)
            {
                tcs.SetResult(null);
            }

            await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_Task_WithTimeout_can_fault_afterwards
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Uncompleted_Task_WithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            try
            {
                await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
                Assert.Fail();
            }
            catch (TimeoutException)
            {
                tcs.SetException(new InvalidOperationException());
            }

            await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_Task_WithTimeout_can_be_canceled_afterwards
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Uncompleted_Task_WithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            try
            {
                await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
                Assert.Fail();
            }
            catch (TimeoutException)
            {
                tcs.SetCanceled();
            }

            await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_TaskOfInt_WithTimeout_can_complete_afterwards
        [TestMethod]
        public async Task Uncompleted_TaskOfInt_WithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            try
            {
                await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
                Assert.Fail();
            }
            catch (TimeoutException)
            {
                tcs.SetResult(36);
            }

            Assert.AreEqual(36, await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500)));
        }
        #endregion

        #region Uncompleted_TaskOfInt_WithTimeout_can_fault_afterwards
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Uncompleted_TaskOfInt_WithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            try
            {
                await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
                Assert.Fail();
            }
            catch (TimeoutException)
            {
                tcs.SetException(new InvalidOperationException());
            }

            await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_TaskOfInt_WithTimeout_can_be_canceled_afterwards
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Uncompleted_TaskOfInt_WithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            try
            {
                await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
                Assert.Fail();
            }
            catch (TimeoutException)
            {
                tcs.SetCanceled();
            }

            await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion
    }
}
