// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class Task_WithCancellation_Test
    {
        #region WithCancellation_throws_if_cancelled_after_call
        [Fact]
        public async Task WithCancellation_throws_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Unit>();
            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cts.Cancel();

            cancellationTask
                .Awaiting(_ => _)
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region WithCancellation_throws_if_cancelled_before_call
        [Fact]
        public async Task WithCancellation_throws_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Unit>();

            cts.Cancel();

            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cancellationTask
                .Awaiting(_ => _)
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region WithCancellation_succeeds_if_not_cancelled
        [Fact]
        public async Task WithCancellation_succeeds_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() => Thread.Sleep(100));
            var cts = new CancellationTokenSource();

            await task.WithCancellation(cts.Token);
        }
        #endregion

        #region WithCancellation_with_TaskOfInt_throws_if_cancelled_after_call
        [Fact]
        public async Task WithCancellation_with_TaskOfInt_throws_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();
            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cts.Cancel();

            cancellationTask
                .Awaiting(_ => _)
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region WithCancellation_with_TaskOfInt_throws_if_cancelled_before_call
        [Fact]
        public async Task WithCancellation_with_TaskOfInt_throws_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();

            cts.Cancel();

            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cancellationTask
                .Awaiting(_ => _)
                .ShouldThrowExactly<TaskCanceledException>();
        }
        #endregion

        #region WithCancellation_with_TaskOfInt_succeeds_if_not_cancelled
        [Fact]
        public async Task WithCancellation_with_TaskOfInt_succeeds_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                return 36;
            });

            var cts = new CancellationTokenSource();

            Assert.Equal(36, await task.WithCancellation(cts.Token));
        }
        #endregion

        #region Exceptions_Are_Propagated_Through_WithCancellation_Of_Task
        [Fact]
        public async Task Exceptions_Are_Propagated_Through_WithCancellation_Of_Task()
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

        #region Exceptions_Are_Propagated_Through_WithCancellation_With_Task_Of_Int
        [Fact]
        public async Task Exceptions_Are_Propagated_Through_WithCancellation_With_Task_Of_Int()
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

        #region Exceptions_Are_Propagated_Through_Nested_TryWithCancellation_With_Task_Of_Int
        [Fact]
        public async Task Exceptions_Are_Propagated_Through_Nested_TryWithCancellation_With_Task_Of_Int()
        {
            var ex = new ApplicationException();

            Func<Task<int>> faultingTaskFunc = async () =>
            {
                throw ex;
            };

            faultingTaskFunc
                .Awaiting(_ => _()
                    .TryWithCancellation(CancellationToken.None)
                    .TryWithCancellation(CancellationToken.None))
                .ShouldThrowExactly<ApplicationException>()
                .Where(ex2 => ex == ex2);
        }
        #endregion
    }
}