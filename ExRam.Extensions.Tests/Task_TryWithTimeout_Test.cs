// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monad;

namespace ExRam.Framework.Tests
{
    [TestClass]
    public class Task_TryWithTimeout_Test
    {
        #region Completed_Task_TryWithTimeout_Completes
        [TestMethod]
        public async Task Completed_Task_TryWithTimeout_Completes()
        {
            var completedTask = Task.FromResult(0);
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Completed_TaskOfInt_TryWithTimeout_Completes
        [TestMethod]
        public async Task Completed_TaskOfInt_TryWithTimeout_Completes()
        {
            var completedTask = Task.FromResult(36);
            Assert.AreEqual(36, await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500)));
        }
        #endregion

        #region Completed_TaskOfMaybeOfInt_TryWithTimeout_Completes
        [TestMethod]
        public async Task Completed_TaskOfMaybeOfInt_TryWithTimeout_Completes()
        {
            var completedTask = Task.FromResult<OptionStrict<int>>(36);
            Assert.AreEqual(36, (await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).Value);
        }
        #endregion

        #region Faulted_Task_TryWithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Faulted_Task_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<Unit>(new InvalidOperationException());
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Faulted_TaskOfInt_TryWithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Faulted_TaskOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<int>(new InvalidOperationException());
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Faulted_TaskOfMaybeOfInt_TryWithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Faulted_TaskOfMaybeOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<OptionStrict<int>>(new InvalidOperationException());
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Canceled_Task_TryWithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Canceled_Task_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<Unit>();
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Canceled_TaskOfInt_TryWithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Canceled_TaskOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<int>();
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Canceled_TaskOfMaybeOfInt_TryWithTimeout_Faults
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Canceled_TaskOfMaybeOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<OptionStrict<int>>();
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_Task_TryWithTimeout_returns_unset_Maybe
        [TestMethod]
        public async Task Uncompleted_Task_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<Unit>();
            Assert.IsFalse((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
        }
        #endregion

        #region Uncompleted_TaskOfInt_TryWithTimeout_returns_unset_Maybe
        [TestMethod]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<int>();
            Assert.IsFalse((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
        }
        #endregion

        #region Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_returns_unset_Maybe
        [TestMethod]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<OptionStrict<int>>();
            Assert.IsFalse((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
        }
        #endregion

        #region Uncompleted_Task_TryWithTimeout_can_complete_afterwards
        [TestMethod]
        public async Task Uncompleted_Task_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.IsFalse((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
            tcs.SetResult(new object());

            var next = await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            Assert.IsTrue((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
        }
        #endregion

        #region Uncompleted_Task_TryWithTimeout_can_fault_afterwards
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Uncompleted_Task_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.IsFalse((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
            tcs.SetException(new InvalidOperationException());
            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_Task_TryWithTimeout_can_be_canceled_afterwards
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Uncompleted_Task_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.IsFalse((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
            tcs.SetCanceled();
            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_TaskOfInt_TryWithTimeout_can_complete_afterwards
        [TestMethod]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            Assert.IsFalse((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
            tcs.SetResult(36);
            Assert.IsTrue((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
        }
        #endregion

        #region Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_complete_afterwards
        [TestMethod]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<OptionStrict<int>>();

            Assert.IsFalse((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
            tcs.SetResult(36);
            Assert.IsTrue((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
        }
        #endregion

        #region Uncompleted_TaskOfInt_TryWithTimeout_can_fault_afterwards
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            tcs.SetException(new InvalidOperationException());
            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_fault_afterwards
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<OptionStrict<int>>();

            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            tcs.SetException(new InvalidOperationException());
            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_TaskOfInt_TryWithTimeout_can_be_canceled_afterwards
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            Assert.IsFalse((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
            tcs.SetCanceled();
            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_be_canceled_afterwards
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<OptionStrict<int>>();

            Assert.IsFalse((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).HasValue);
            tcs.SetCanceled();
            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion
    }
}
