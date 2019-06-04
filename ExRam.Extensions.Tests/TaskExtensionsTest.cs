// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using LanguageExt;

namespace ExRam.Extensions.Tests
{
    public class TaskExtensionsTest
    {
        [Fact]
        public async Task ToAsyncEnumerable_completes()
        {
            var tcs = new TaskCompletionSource<bool>();

            var task = ((Task)tcs.Task).ToAsyncEnumerable().FirstAsync();

            Assert.False(task.IsCompleted);
            tcs.SetResult(true);

            await task;
        }

        [Fact]
        public async Task ToAsyncEnumerable_forwards_exception()
        {
            var tcs = new TaskCompletionSource<bool>();

            var task = ((Task)tcs.Task)
                .ToAsyncEnumerable()
                .FirstAsync();

            Assert.False(task.IsCompleted);
            tcs.SetException(new DivideByZeroException());

            task
                .Awaiting(_ => _.AsTask())
                .Should()
                .ThrowExactly<AggregateException>()
                .Where(ex => ex.GetBaseException() is DivideByZeroException);
        }

        [Fact]
        public async Task TryWithCancellation_asynchronously_returns_false_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Unit>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.False((await cancellationTask).IsSome);
        }

        [Fact]
        public async Task TryWithCancellation_asynchronously_returns_false_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Unit>();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.False((await cancellationTask).IsSome);
        }

        [Fact]
        public void TryWithCancellation_asynchronously_returns_true_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() => Thread.Sleep(100));
            var cts = new CancellationTokenSource();

            Assert.Equal(true, task.TryWithCancellation(cts.Token).Result);
        }

        [Fact]
        public async Task TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.False((await cancellationTask).IsSome);
        }

        [Fact]
        public async Task TryWithCancellation_with_TaskOfInt_returns_correct_Maybe_value_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.False((await cancellationTask).IsSome);
        }

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

        [Fact]
        public async Task TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Option<int>>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cts.Cancel();

            Assert.False((await cancellationTask).IsSome);
        }

        [Fact]
        public async Task TryWithCancellation_with_TaskOfMaybe_returns_correct_Maybe_value_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Option<int>>();

            cts.Cancel();

            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            Assert.False((await cancellationTask).IsSome);
        }

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

        [Fact]
        public async Task TryWithCancellation_throws_if_called_on_cancelled_task()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetCanceled<Unit>();
            var cancellationTask = longRunningTask.TryWithCancellation(cts.Token);

            cancellationTask
                .Awaiting(_ => _)
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

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
                .Should()
                .ThrowExactly<ApplicationException>()
                .Where(ex2 => ex == ex2);
        }

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
                .Should()
                .ThrowExactly<ApplicationException>()
                .Where(ex2 => ex == ex2);
        }

        [Fact]
        public async Task Completed_Task_TryWithTimeout_Completes()
        {
            var completedTask = Task.FromResult(0);
            await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500));
        }

        [Fact]
        public async Task Completed_TaskOfInt_TryWithTimeout_Completes()
        {
            var completedTask = Task.FromResult(36);
            Assert.Equal(36, await completedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500)));
        }

        [Fact]
        public async Task Completed_TaskOfMaybeOfInt_TryWithTimeout_Completes()
        {
            var maybeValue = await Task
                .FromResult<Option<int>>(36)
                .TryWithTimeout(TimeSpan.FromMilliseconds(500));

            maybeValue.IsSome.Should().BeTrue();
            maybeValue.IfSome(val => val.Should().Be(36));
        }

        [Fact]
        public async Task Faulted_Task_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<Unit>(new DivideByZeroException());

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Faulted_TaskOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<int>(new DivideByZeroException());

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Faulted_TaskOfMaybeOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<Option<int>>(new DivideByZeroException());

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Canceled_Task_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<Unit>();

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Canceled_TaskOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<int>();

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Canceled_TaskOfMaybeOfInt_TryWithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<Option<int>>();

            completedTask
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Uncompleted_Task_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<Unit>();
            Assert.False((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<int>();
            Assert.False((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }

        [Fact]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_returns_unset_Maybe()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<Option<int>>();
            Assert.False((await uncompletedTask.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }

        [Fact]
        public async Task Uncompleted_Task_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetResult(new object());

            var next = await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            Assert.True((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }

        [Fact]
        public async Task Uncompleted_Task_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetException(new DivideByZeroException());

            tcs
                .Awaiting(_ => _.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Uncompleted_Task_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetCanceled();

            tcs.Task
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetResult(36);
            Assert.True((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }

        [Fact]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<Option<int>>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetResult(36);
            Assert.True((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            tcs.SetException(new DivideByZeroException());

            tcs.Task
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<Option<int>>();

            await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500));
            tcs.SetException(new DivideByZeroException());

            tcs.Task
                .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetCanceled();

            tcs.Task
               .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
               .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfMaybeOfInt_TryWithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<Option<int>>();

            Assert.False((await tcs.Task.TryWithTimeout(TimeSpan.FromMilliseconds(500))).IsSome);
            tcs.SetCanceled();

            tcs.Task
               .Awaiting(_ => _.TryWithTimeout(TimeSpan.FromMilliseconds(500)))
               .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task WithCancellation_throws_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Unit>();
            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cts.Cancel();

            cancellationTask
                .Awaiting(_ => _)
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task WithCancellation_throws_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<Unit>();

            cts.Cancel();

            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cancellationTask
                .Awaiting(_ => _)
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task WithCancellation_succeeds_if_not_cancelled()
        {
            var task = Task.Factory.StartNew(() => Thread.Sleep(100));
            var cts = new CancellationTokenSource();

            await task.WithCancellation(cts.Token);
        }

        [Fact]
        public async Task WithCancellation_with_TaskOfInt_throws_if_cancelled_after_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();
            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cts.Cancel();

            cancellationTask
                .Awaiting(_ => _)
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task WithCancellation_with_TaskOfInt_throws_if_cancelled_before_call()
        {
            var cts = new CancellationTokenSource();
            var longRunningTask = Task.Factory.GetUncompleted<int>();

            cts.Cancel();

            var cancellationTask = longRunningTask.WithCancellation(cts.Token);

            cancellationTask
                .Awaiting(_ => _)
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

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
                .Should()
                .ThrowExactly<ApplicationException>()
                .Where(ex2 => ex == ex2);
        }

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
                .Should()
                .ThrowExactly<ApplicationException>()
                .Where(ex2 => ex == ex2);
        }

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
                .Should()
                .ThrowExactly<ApplicationException>()
                .Where(ex2 => ex == ex2);
        }

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
            var completedTask = Task.Factory.GetFaulted<Unit>(new DivideByZeroException());

            completedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Faulted_TaskOfInt_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetFaulted<int>(new DivideByZeroException());

            completedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Canceled_Task_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<Unit>();

            completedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Canceled_TaskOfInt_WithTimeout_Faults()
        {
            var completedTask = Task.Factory.GetCanceled<int>();

            completedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TaskCanceledException>();
        }

        [Fact]
        public async Task Uncompleted_Task_WithTimeout_faults_with_TimeoutException()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<Unit>();

            uncompletedTask
               .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
               .Should()
                .ThrowExactly<TimeoutException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_WithTimeout_faults_with_TimeoutException()
        {
            var uncompletedTask = Task.Factory.GetUncompleted<int>();

            uncompletedTask
                .Awaiting(_ => _.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TimeoutException>();
        }

        [Fact]
        public async Task Uncompleted_Task_WithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TimeoutException>();
        }

        [Fact]
        public async Task Uncompleted_Task_WithTimeout_can_fault_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TimeoutException>();

            tcs.SetException(new DivideByZeroException());

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Uncompleted_Task_WithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<object>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TimeoutException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_WithTimeout_can_complete_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TimeoutException>();

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
                .Should()
                .ThrowExactly<TimeoutException>();

            tcs.SetException(new DivideByZeroException());

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<DivideByZeroException>();
        }

        [Fact]
        public async Task Uncompleted_TaskOfInt_WithTimeout_can_be_canceled_afterwards()
        {
            var tcs = new TaskCompletionSource<int>();

            tcs
                .Awaiting(_ => _.Task.WithTimeout(TimeSpan.FromMilliseconds(500)))
                .Should()
                .ThrowExactly<TimeoutException>();
        }
    }
}
