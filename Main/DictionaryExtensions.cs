using System.Collections.Generic;
using System.Linq;

namespace EngTranslatorMod.Main
{
    public static class DictionaryExtensions
    {
        // Works in C#3/VS2008:
        // Returns a new dictionary of this ... others merged leftward.
        // Keeps the type of 'this', which must be default-instantiable.
        // Example: 
        //   result = map.MergeLeft(other1, other2, ...)
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>, new()
        {
            T newMap = new T();
            foreach (IDictionary<K, V> src in
                new List<IDictionary<K, V>> { me }.Concat(others))
            {
                // ^-- echk. Not quite there type-system.
                foreach (KeyValuePair<K, V> p in src)
                {
                    newMap[p.Key] = p.Value;
                }
            }
            return newMap;
        }

        public static Dictionary<TKey, TValue>
        Merge<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>(dictionaries.First().Comparer);
            foreach (Dictionary<TKey, TValue> dict in dictionaries)
                foreach (KeyValuePair<TKey, TValue> x in dict)
                    result[x.Key] = x.Value;
            return result;
        }
    }
}