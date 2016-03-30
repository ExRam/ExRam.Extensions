using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ExRam.Extensions.Tests
{
    public class CancellationTokenExtensions_Test
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
    }
}
