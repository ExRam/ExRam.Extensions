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
    public class Task_Select_Test
    {
        #region Select_on_completed_TaskOfInt_succeeds
        [TestMethod]
        public async Task Select_on_completed_TaskOfInt_succeeds()
        {
            var task1 = Task.FromResult(1);
            var task2 = task1.Select(x => x + 1);

            Assert.AreEqual(2, await task2);
        }
        #endregion

        #region Select_on_faulted_TaskOfInt_throws
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task Select_on_faulted_TaskOfInt_throws()
        {
            var task1 = Task.Factory.GetFaulted<int>(new NotSupportedException());
            var task2 = task1.Select(x => x + 1);

            await task2;
        }
        #endregion

        #region Select_on_canceled_TaskOfInt_throws
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Select_on_canceled_TaskOfInt_throws()
        {
            var task1 = Task.Factory.GetCanceled<int>();
            var task2 = task1.Select(x => x + 1);

            await task2;
        }
        #endregion

        #region Throwing_Selector_throws_on_projected_completed_TaskOfInt
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task Throwing_Selector_throws_on_projected_completed_TaskOfInt()
        {
            var task1 = Task.FromResult(1);
            var task2 = task1.Select(x =>
            {
                throw new NotSupportedException();
                return x + 1;
            });

            await task2;
        }
        #endregion

        #region Throwing_Selector_throws_on_projected_faulted_TaskOfInt
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public async Task Throwing_Selector_throws_on_projected_faulted_TaskOfInt()
        {
            var task1 = Task.Factory.GetFaulted<int>(new InvalidCastException());
            var task2 = task1.Select(x =>
            {
                throw new NotSupportedException();
                return x + 1;
            });

            await task2;
        }
        #endregion

        #region Throwing_Selector_throws_on_projected_canceled_TaskOfInt
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Throwing_Selector_throws_on_projected_canceled_TaskOfInt()
        {
            var task1 = Task.Factory.GetCanceled<int>();
            var task2 = task1.Select(x =>
            {
                throw new NotSupportedException();
                return x + 1;
            });

            await task2;
        }
        #endregion

        #region Select_on_completed_Task_succeeds
        [TestMethod]
        public async Task Select_on_completed_Task_succeeds()
        {
            var task1 = Task.Factory.GetCompleted();
            var task2 = task1.Select(() => 1);

            Assert.AreEqual(1, await task2);
        }
        #endregion

        #region Select_on_faulted_Task_throws
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task Select_on_faulted_Task_throws()
        {
            var task1 = Task.Factory.GetFaulted(new NotSupportedException());
            var task2 = task1.Select(() => 1);

            await task2;
        }
        #endregion

        #region Select_on_canceled_Task_throws
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Select_on_canceled_Task_throws()
        {
            var task1 = Task.Factory.GetCanceled();
            var task2 = task1.Select(() => 1);

            await task2;
        }
        #endregion

        #region Throwing_Selector_throws_on_projected_completed_task
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public async Task Throwing_Selector_throws_on_projected_completed_task()
        {
            var task1 = Task.Factory.GetCompleted();
            var task2 = task1.Select(() =>
            {
                throw new NotSupportedException();
                return 1;
            });

            await task2;
        }
        #endregion

        #region Throwing_Selector_throws_on_projected_faulted_task
        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public async Task Throwing_Selector_throws_on_projected_faulted_task()
        {
            var task1 = Task.Factory.GetFaulted(new InvalidCastException());
            var task2 = task1.Select(() =>
            {
                throw new NotSupportedException();
                return 1;
            });

            await task2;
        }
        #endregion

        #region Throwing_Selector_throws_on_projected_canceled_task
        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task Throwing_Selector_throws_on_projected_canceled_task()
        {
            var task1 = Task.Factory.GetCanceled();
            var task2 = task1.Select(() =>
            {
                throw new NotSupportedException();
                return 1;
            });

            await task2;
        }
        #endregion
    }
}
