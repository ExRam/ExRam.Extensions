using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Threading
{
    public static class CancellationTokenExtensions
    {
        public static async Task ToTask(this CancellationToken ct, CancellationToken differentCt)
        {
            var tcs = new TaskCompletionSource<Unit>();

            using (ct.Register(() => tcs.TrySetResult(Unit.Default)))
            {
                if (differentCt.CanBeCanceled)
                {
                    var innerRegistration = differentCt.Register(() =>
                    {
                        if (!ct.IsCancellationRequested)
                            tcs.TrySetCanceled();
                    });

                    using (innerRegistration)
                    {
                        await tcs.Task.ConfigureAwait(false);
                    }
                }
                else
                {
                    await tcs.Task.ConfigureAwait(false);
                }
            }
        }

        public static IObservable<Unit> ToObservable(this CancellationToken ct)
        {
            return ct.ToObservable(Scheduler.Default);
        }

        public static IObservable<Unit> ToObservable(this CancellationToken ct, IScheduler scheduler)
        {
            return Observable
                .Create<Unit>(observer =>
                    ct.Register(() => 
                        scheduler.Schedule(() =>
                        {
                            observer.OnNext(Unit.Default);
                            observer.OnCompleted();
                        })));
        }
    }
}
