using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
            return Observable.Defer(() =>
            {
                var tcs = new TaskCompletionSource<bool>();

                return Observable.Using(
                    () => new CompositeDisposable(
                        ct.Register(() => tcs.TrySetResult(true)),
                        Disposable.Create(() => tcs.TrySetResult(false))),
                    disposable => tcs.Task
                        .ToObservable()
                        .Where(value => value)
                        .Select(dummy => Unit.Default));
            });
        }
    }
}
