using System.Collections.Generic;

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

            return array[Rnd.Next(count)];
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T appendElement)
        {
            return source.Concat(new[] { appendElement });
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(t => !ReferenceEquals(t, default(T)));
        }
    }
}