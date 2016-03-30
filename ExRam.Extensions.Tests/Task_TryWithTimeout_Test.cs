// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Threading.Tasks;
using LanguageExt;
using Xunit;
using Unit = System.Reactive.Unit;
using FluentAssertions;

namespace ExRam.Extensions.Tests
{
    public class Task_TryWithTimeout_Test
    {
        #region Completed_Task_TryWithTimeout_Completes
        [Fact]
        public async Task Completed_Task_TryWithTimeout_Completes()
        {
            var completedTask = Task.FromResult(0);
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }
        #endregion

        #region Completed_TaskOfInt_TryWithTimeout_Completes
        [Fact]
        public async Task Completed_TaskOfInt_TryWithTimeout_Completes()
        {
            var completedTask = Task.FromResult(36);
            Assert.Equal(36, await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500)));
        }
        #endregion

        #region Completed_TaskOfMaybeOfInt_TryWithTimeout_Completes
        [Fact]
        public async Task Completed_TaskOfMaybeOfInt_TryWithTimeout_Completes()
        {
            var completedTask = Task.FromResult<Option<int>>(36);
            Assert.Equal(36, (await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).GetValue());
        }
        #endregion

        #region Faulted_Task_TryWithTimeout_Faults
        [Fact]
        public async Task Faulted_Task_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<Unit>(new InvalidOperationException());

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }
        #endregion

        #region Faulted_TaskOfInt_TryWithTimeout_Faults
        [Fact]
        public async Task Faulted_TaskOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<int>(new InvalidOperationException());

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }
        #endregion

        #region Faulted_TaskOfMaybeOfInt_TryWithTimeout_Faults
        [Fact]
        public async Task Faulted_TaskOfMaybeOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<Option<int>>(new InvalidOperationException());

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }
        #endregion

        #region Canceled_Task_TryWithTimeout_Faults
        [Fact]
        public async Task Canceled_Task_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<Unit>();

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region Canceled_TaskOfInt_TryWithTimeout_Faults
        [Fact]
        public async Task Canceled_TaskOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<int>();

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region Canceled_TaskOfMaybeOfInt_TryWithTimeout_Faults
        [Fact]
        public async Task Canceled_TaskOfMaybeOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<Option<int>>();

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region Uncompleted_Task_TryWithTimeout_returns_unset_Maybe
        [Fact]
        public async Task Uncompleted_Task_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<Unit>();
            Assert.False((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }
        #endregion

        #region Uncompleted_TaskOfInt_TryWithTimeout_returns_unset_Maybe
        [Fact]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<int>();
            Assert.False((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }
        #endregion

        #region Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_returns_unset_Maybe
        [Fact]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<Option<int>>();
            Assert.False((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }
        #endregion

        #region Uncompleted_Task_TryWithTimeout_can_complete_afterwards
        [Fact]
        public async Task Uncompleted_Task_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetResult(new object());

            var next = await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            Assert.True((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }
        #endregion

        #region Uncompleted_Task_TryWithTimeout_can_fault_afterwards
        [Fact]
        public async Task Uncompleted_Task_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetException(new InvalidOperationException());

            tcs.Task
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }
        #endregion

        #region Uncompleted_Task_TryWithTimeout_can_be_canceled_afterwards
        [Fact]
        public async Task Uncompleted_Task_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetCanceled();

            tcs.Task
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region Uncompleted_TaskOfInt_TryWithTimeout_can_complete_afterwards
        [Fact]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetResult(36);
            Assert.True((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }
        #endregion

        #region Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_complete_afterwards
        [Fact]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<Option<int>>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetResult(36);
            Assert.True((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }
        #endregion

        #region Uncompleted_TaskOfInt_TryWithTimeout_can_fault_afterwards
        [Fact]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            tcs.SetException(new InvalidOperationException());

            tcs.Task
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }
        #endregion

        #region Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_fault_afterwards
        [Fact]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<Option<int>>();

            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            tcs.SetException(new InvalidOperationException());

            tcs.Task
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }
        #endregion

        #region Uncompleted_TaskOfInt_TryWithTimeout_can_be_canceled_afterwards
        [Fact]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetCanceled();

            tcs.Task
               .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
               .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_be_canceled_afterwards
        [Fact]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<Option<int>>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetCanceled();

            tcs.Task
               .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
               .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion
    }
}
