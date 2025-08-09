using System.Collections;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation;

public class CachedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
where TKey:notnull
{
    private readonly KeyValuePair<TKey, TValue>[] _cache;
    private int _index = 0;
    private int _populated = 0;
    private readonly Dictionary<TKey, TValue> _backing = new();

    public CachedDictionary(int size)
    {
        _cache = new KeyValuePair<TKey, TValue>[size];
       
    }

    public static int CacheHits;
    public static int Accesses;
    public bool TryGetValue(TKey key,out TValue value)
    {
        Accesses++;
        for (var i = 0; i < _cache.Length; i++)
        {
            var effective = (i + _index) % _cache.Length;
            if (effective >= _populated)
                break;
            if (_cache[effective]!.Key!.Equals(key))
            {
                CacheHits++;
                value = _cache[effective].Value;
                return true;
            }
        }
        //if it's not in the cache, try seaching the backing..
        if (_backing.TryGetValue(key, out value!))
        {   //Reinsert to cache
            AddToCache(key,value);
            return true;
        }

        value = default!;
        return false;
    }

    private void AddToCache(TKey key, TValue value)
    {
        _populated++;
        if (_cache.Length == 0)
            return;
        _cache[_index] = new KeyValuePair<TKey, TValue>(key, value);
        _index = (_index + 1) % _cache.Length;

    }
    public void Add(TKey key, TValue value)
    {
        _backing[key] = value;
        AddToCache(key, value);
    }


    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var kvp in _backing) yield return kvp;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<TValue> Values => _backing.Values;
    public IEnumerable<TKey> Keys => _backing.Keys;
}
