namespace System.Collections.Generic
{
    public static class ListExtensions
    {
        public static void AddRange<TElement>(this IList<TElement> list, IEnumerable<TElement> items)
        {
            foreach (var element in items)
            {
                list.Add(element);
            }
        }
    }
}