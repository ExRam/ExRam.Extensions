using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Linq
{
    public static class EnumerableExtensions
    {
        private static readonly Random Rnd = new Random(DateTime.Now.Millisecond);

        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            var array = ((source as T[]) ?? (source.ToArray()));
            var count = array.Length;

            if (count == 0)
                throw new InvalidOperationException();

            return array[EnumerableExtensions.Rnd.Next(count)];
        }

        #region Append
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T appendElement)
        {
            Contract.Requires(source != null);

            return source.Concat(new[] { appendElement });
        }
        #endregion

        #region WhereNotNull
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
        {
            Contract.Requires(source != null);

            return source.Where(t => !object.ReferenceEquals(t, default(T)));
        }
        #endregion
    }
}