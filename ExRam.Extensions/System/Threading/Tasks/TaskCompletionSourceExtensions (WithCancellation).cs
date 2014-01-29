using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static partial class TaskCompletionSourceExtensions
    {
        public static TaskCompletionSource<TResult> WithCancellation<TResult>(this TaskCompletionSource<TResult> tcs, CancellationToken token)
        {
            Contract.Requires(tcs != null);

            if (token.IsCancellationRequested)
                tcs.TrySetCanceled();
            else
            {
                var registration = token.Register(() => tcs.TrySetCanceled());
                tcs.Task.ContinueWith((task2) => registration.Dispose());
            }

            return tcs;
        }
    }
}
