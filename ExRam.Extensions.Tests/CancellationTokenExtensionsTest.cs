using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class CancellationTokenExtensionsTest
    {
        [Fact]
        public async Task ToObservable_produces_value_on_cancellation()
        {
            var cts = new CancellationTokenSource();
            // ReSharper disable once MethodSupportsCancellation
            var unitTask = cts.Token.ToObservable().FirstAsync().ToTask();

            Assert.False(unitTask.IsCompleted);

            cts.Cancel();

            await unitTask;
        }

        [Fact]
        public async Task ToObservable_produces_value_after_cancellation()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // ReSharper disable once MethodSupportsCancellation
            var unitTask = cts.Token.ToObservable().FirstAsync().ToTask();

            await unitTask;
        }

        [Fact]
        public async Task ToTask_different_CancellationTokens_1()
        {
            var cts1 = new CancellationTokenSource();
            var cts2 = new CancellationTokenSource();

            var task = cts1.Token.ToTask(cts2.Token);
            cts1.Cancel();
            cts2.Cancel();

            task
                .Awaiting(x => x)
                .Should()
                .NotThrow();
        }

        [Fact]
        public async Task ToTask_different_CancellationTokens_2()
        {
            var cts1 = new CancellationTokenSource();
            var cts2 = new CancellationTokenSource();

            var task = cts1.Token.ToTask(cts2.Token);
            cts2.Cancel();
            cts1.Cancel();

            task
                .Awaiting(x => x)
                .Should()
                .Throw<TaskCanceledException>();
        }

        [Fact]
        public async Task ToTask_same_CancellationTokens_2()
        {
            var cts1 = new CancellationTokenSource();

            var task = cts1.Token.ToTask(cts1.Token);
            cts1.Cancel();

            task
                .Awaiting(x => x)
                .Should()
                .NotThrow();
        }
    }
}
