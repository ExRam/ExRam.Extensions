// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.Reactive;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace ExRam.Extensions.Tests
{
    public class Task_WithTimeout_Test
    {
        [Fact]
        public async Task Completed_Task_WithTimeout_Completes()
        {
            var completedTask = Task.FromResult(0);
            await completedTask.WithTimeout(TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public async Task Completed_TaskOfInt_WithTimeout_Completes()
        {
            var completedTask = Task.FromResult(36);
            Assert.Equal(36, await completedTask.WithTimeout(TimeSpan.FromMilliseconds(500)));
        }

        [Fact]
        public async Task Faulted_Task_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<Unit>(new InvalidOperationException());

            completedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public async Task Faulted_TaskOfInt_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<int>(new InvalidOperationException());

            completedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public async Task Canceled_Task_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<Unit>();

            completedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Canceled_TaskOfInt_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<int>();

            completedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Uncompleted_Task_WithTimeout_faults_with_TimeoutException()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<Unit>();

            uncompletedTask
               .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
               .ShouldThrowExactly<TimeoutException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_WithTimeout_faults_with_TimeoutException()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<int>();

            uncompletedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TimeoutException>();
        }

        [Fact]
        public async Task Uncompleted_Task_WithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TimeoutException>();
        }

        [Fact]
        public async Task Uncompleted_Task_WithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TimeoutException>();

            tcs.SetException(new InvalidOperationException());

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public async Task Uncompleted_Task_WithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TimeoutException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_WithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TimeoutException>();

            tcs.SetResult(36);

            (await tcs.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .Be(36);
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_WithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TimeoutException>();

            tcs.SetException(new InvalidOperationException());

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_WithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .ShouldThrowExactly<TimeoutException>();
        }
    }
}
