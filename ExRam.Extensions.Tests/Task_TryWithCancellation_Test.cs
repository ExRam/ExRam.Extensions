// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using Xunit;
using Unit = System.Reactive.Unit;

namespace ExRam.Extensions.Tests
{
    public class Task_TryWithCancellation_Test
    {
        #region TryWithCancellation_asynchronously_returns_false_if_cancelled_after_call
        [Fact]
        public async Task TryWithCancellation_asynchronously_returns_false_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Unit>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.False((await cancellationTask).IsSome);
        }
        #endregion

        #region TryWithCancellation_asynchronously_returns_false_if_cancelled_before_call
        [Fact]
        public async Task TryWithCancellation_asynchronously_returns_false_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Unit>();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.False((await cancellationTask).IsSome);
        }
        #endregion

        #region TryWithCancellation_asynchronously_returns_true_if_not_cancelled
        [Fact]
        public void TryWithCancellation_asynchronously_returns_true_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() => Thread.Sleep(100));
            var cts = new CancellationTokenSource();

            Assert.Equal(true, task.TryWithCancellation(cts.Token).Result);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_after_call
        [Fact]
        public async Task TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.False((await cancellationTask).IsSome);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_before_call
        [Fact]
        public async Task TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.False((await cancellationTask).IsSome);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_not_cancelled
        [Fact]
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

            maybeInt.IsSome.Should().BeTrue();
            maybeInt.IfSome(val =>
            {
                val.Should().Be(36);
            });
        }
        #endregion

        #region TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_after_call
        [Fact]
        public async Task TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Option<int>>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.False((await cancellationTask).IsSome);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_before_call
        [Fact]
        public async Task TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Option<int>>();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.False((await cancellationTask).IsSome);
        }
        #endregion

        #region TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_not_cancelled
        [Fact]
        public async Task TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                return (Option<int>)36;
            });

            var cts = new CancellationTokenSource();

            var maybeInt = await task.TryWithCancellation(cts.Token);

            maybeInt.IsSome.Should().BeTrue();
            maybeInt.IfSome(val =>
            {
                val.Should().Be(36);
            });
        }
        #endregion

        #region TryWithCancellation_throws_if_called_on_cancelled_task
        [Fact]
        public async Task TryWithCancellation_throws_if_called_on_cancelled_task()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetCanceled<Unit>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cancellationTask
                .Awaiting(_ => _)
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region Exceptions_Are_Propagated_Through_TryWithCancellation_Of_Task
        [Fact]
        public async Task Exceptions_Are_Propagated_Through_TryWithCancellation_Of_Task()
        {
            var ex = new ApplicationException();

            Func<Task> faultingTaskFunc = async () =>
            {
                throw ex;
            };

            faultingTaskFunc
                .Awaiting(_ => _().TryWithCancellation(CancellationToken.None))
                .ShouldThrowExactly<ApplicationException>()
                .Where(ex2 => ex == ex2);
        }
        #endregion

        #region Exceptions_Are_Propagated_Through_TryWithCancellation_With_Task_Of_Int
        [Fact]
        public async Task Exceptions_Are_Propagated_Through_TryWithCancellation_With_Task_Of_Int()
        {
            var ex = new ApplicationException();

            Func<Task<int>> faultingTaskFunc = async () =>
            {
                throw ex;
            };

            faultingTaskFunc
                .Awaiting(_ => _().TryWithCancellation(CancellationToken.None))
                .ShouldThrowExactly<ApplicationException>()
                .Where(ex2 => ex == ex2);
        }
        #endregion
    }
}