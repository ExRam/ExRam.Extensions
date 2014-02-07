using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Threading
{
    public static class CancellationTokenExtensions
    {
        public static Task ToTask(this CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<object>();
            ct.Register(() => tcs.SetResult(null));

            return tcs.Task;
        }

        public static IObservable<Unit> ToObservable(this CancellationToken ct)
        {
            return Observable.Create<Unit>((observer) => ct.Register(() =>
            {
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }));
        }
    }
}
