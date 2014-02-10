using System.Diagnostics.Contracts;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            Contract.Requires(dictionary != null);

            return dictionary.GetValueOrDefault(key, default(TValue));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            Contract.Requires(dictionary != null);

            TValue ret;
            return ((dictionary.TryGetValue(key, out ret)) ? (ret) : (defaultValue));
        }

        public static Maybe<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            Contract.Requires(dictionary != null);

            TValue item;
            return ((dictionary.TryGetValue(key, out item)) ? (item) : (Maybe<TValue>.Null));
        }
    }
}
