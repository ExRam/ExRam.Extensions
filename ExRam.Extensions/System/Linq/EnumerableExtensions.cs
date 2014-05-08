using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        #region ForEach
        public static void ForEach<T>(this IEnumerable<T> array, Action<T> action)
        {
            Contract.Requires(array != null);

            foreach (var obj in array)
            {
                action(obj);
            }
        }
        #endregion
    }
}
