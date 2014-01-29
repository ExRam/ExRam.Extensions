using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    public static partial class TaskExtensions
    {
        public static async void Ignore(this Task task)
        {
            Contract.Requires(task != null);

            await task.Swallow();
        }
    }
}
