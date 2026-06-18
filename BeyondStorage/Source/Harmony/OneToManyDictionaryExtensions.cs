using System.Collections.Generic;

namespace BeyondStorage;

public static class OneToManyDictionaryExtensions
{
    public static List<(TKey key, IReadOnlyCollection<TValue> values)>.Enumerator GetEnumerator<TKey, TValue>(
        this global::OneToManyDictionary<TKey, TValue> dict)
    {
        var result = new List<(TKey, IReadOnlyCollection<TValue>)>(dict.CountKeys);
        foreach (var kvp in dict.m_keyToValues)
        {
            result.Add((kvp.Key.Value, new OneToManyDictionary<TKey, TValue>.ValuesView(kvp.Value)));
        }
        return result.GetEnumerator();
    }
}
