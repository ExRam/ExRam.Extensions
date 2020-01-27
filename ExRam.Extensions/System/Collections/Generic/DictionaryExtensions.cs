using System.Diagnostics.Contracts;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            Contract.Requires(dictionary != null);
            Contract.Requires(!object.ReferenceEquals(key, null));

            return dictionary.GetValueOrDefault(key, default);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            Contract.Requires(dictionary != null);
            Contract.Requires(!object.ReferenceEquals(key, null));

            TValue ret;
            return (dictionary.TryGetValue(key, out ret)) ? ret : defaultValue;
        }
    }
}
