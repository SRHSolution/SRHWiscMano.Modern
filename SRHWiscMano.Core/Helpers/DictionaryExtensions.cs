namespace SRHWiscMano.Core.Helpers
{
    public static class DictionaryExtensions
    {
        public static V GetOrDefault<K, V>(this IDictionary<K, V> dictionary, K key)
        {
            V orDefault = default(V);
            dictionary.TryGetValue(key, out orDefault);
            return orDefault;
        }

        public static IDictionary<K, int> Accumulate<K>(
            this IDictionary<K, int> dictionary,
            K key,
            int value)
        {
            return dictionary.Accumulate<K, int, int>(key, value, (Func<int, int, int>)((e, v) => e + v));
        }

        public static IDictionary<K, List<V>> Accumulate<K, V>(
            this IDictionary<K, List<V>> dictionary,
            K key,
            V value)
        {
            return dictionary.Accumulate<K, List<V>, V>(key, value, (Func<List<V>, V, List<V>>)((e, v) =>
            {
                if (e == null)
                    e = new List<V>();
                e.Add(v);
                return e;
            }));
        }

        public static IDictionary<K, V> Accumulate<K, V, T>(
            this IDictionary<K, V> dictionary,
            K key,
            T value,
            Func<V, T, V> accumulator)
        {
            V v;
            dictionary[key] = !dictionary.TryGetValue(key, out v) ? accumulator(default(V), value) : accumulator(v, value);
            return dictionary;
        }
    }
}
