using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System
{
    public static class TimeSpanExtensions
    {
        public static TaskAwaiter GetAwaiter(this TimeSpan timeSpan)
        {
            return Task.Delay(((timeSpan >= TimeSpan.Zero) ? (timeSpan) : (TimeSpan.Zero))).GetAwaiter();
        }
    }
}
