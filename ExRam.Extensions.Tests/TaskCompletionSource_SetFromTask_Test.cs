// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monad;

namespace ExRam.Framework.Tests
{
    [TestClass]
    public class TaskCompletionSource_SetFromTask_Test
    {
        #region TaskCompletionSource_Can_Be_Set_From_Completed_Task
        [TestMethod]
        public async Task TaskCompletionSource_Can_Be_Set_From_Completed_Task()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetFromTask(Task.Factory.GetCompleted());

            await tcs.Task;
        }
        #endregion

        #region TaskCompletionSource_Can_Be_Set_From_Canceled_Task
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task TaskCompletionSource_Can_Be_Set_From_Canceled_Task()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetFromTask(Task.Factory.GetCanceled());

            await tcs.Task;
        }
        #endregion

        #region TaskCompletionSource_Can_Be_Set_From_Faulted_Task
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task TaskCompletionSource_Can_Be_Set_From_Faulted_Task()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetFromTask(Task.Factory.GetFaulted(new NotSupportedException()));

            await tcs.Task;
        }
        #endregion

        #region TaskCompletionSourceOfInt_Can_Be_Set_From_Completed_TaskOfInt
        [TestMethod]
        public async Task TaskCompletionSourceOfInt_Can_Be_Set_From_Completed_TaskOfInt()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetFromTask(Task.FromResult(36));

            Assert.AreEqual(36, await tcs.Task);
        }
        #endregion

        #region TaskCompletionSourceOfInt_Can_Be_Set_From_Canceled_TaskOfInt
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task TaskCompletionSourceOfInt_Can_Be_Set_From_Canceled_TaskOfInt()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetFromTask(Task.Factory.GetCanceled<int>());

            await tcs.Task;
        }
        #endregion

        #region TaskCompletionSourceOfInt_Can_Be_Set_From_Faulted_TaskOfInt
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task TaskCompletionSourceOfInt_Can_Be_Set_From_Faulted_TaskOfInt()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetFromTask(Task.Factory.GetFaulted<int>(new NotSupportedException()));

            await tcs.Task;
        }
        #endregion

        #region TaskCompletionSourceOfMaybeOfInt_Can_Be_Set_From_Completed_TaskOfInt
        [TestMethod]
        public async Task TaskCompletionSourceOfMaybeOfInt_Can_Be_Set_From_Completed_TaskOfInt()
        {
            var tcs = new TaskCompletionSource<OptionStrict<int>>();
            tcs.SetFromTask(Task.FromResult(36));

            Assert.AreEqual(36, await tcs.Task);
        }
        #endregion

        #region TaskCompletionSourceOfMaybeOfInt_Can_Be_Set_From_Canceled_TaskOfInt
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task TaskCompletionSourceOfMaybeOfInt_Can_Be_Set_From_Canceled_TaskOfInt()
        {
            var tcs = new TaskCompletionSource<OptionStrict<int>>();
            tcs.SetFromTask(Task.Factory.GetCanceled<int>());

            await tcs.Task;
        }
        #endregion

        #region TaskCompletionSourceOfMaybeOfInt_Can_Be_Set_From_Faulted_TaskOfInt
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task TaskCompletionSourceOfMaybeOfInt_Can_Be_Set_From_Faulted_TaskOfInt()
        {
            var tcs = new TaskCompletionSource<OptionStrict<int>>();
            tcs.SetFromTask(Task.Factory.GetFaulted<int>(new NotSupportedException()));

            await tcs.Task;
        }
        #endregion
    }
}