using System.Reactive;
using System.Reactive.Linq;

namespace System.Threading
{
    public static class CancellationTokenExtensions
    {
        public static IObservable<Unit> ToObservable(this CancellationToken ct)
        {
            return Observable
                .Create<Unit>(observer => ct.Register(() =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }));
        }
    }
}
