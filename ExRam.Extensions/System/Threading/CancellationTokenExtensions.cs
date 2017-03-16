using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace System.Threading
{
    public static class CancellationTokenExtensions
    {
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
