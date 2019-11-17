/*using System;
using System.Collections;
using System.Collections.Generic;

internal class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    Dictionary<WeakReference, TValue> dictionary = new Dictionary<WeakReference, TValue>(new Comparer());

    private class Comparer : IEqualityComparer<WeakReference>
    {
        bool IEqualityComparer<WeakReference>.Equals(WeakReference x, WeakReference y)
        {
            return x.IsAlive && y.IsAlive && x.Target == y.Target;
        }

        int IEqualityComparer<WeakReference>.GetHashCode(WeakReference obj)
        {
            return obj.Target.GetHashCode();
        }
    }

    private class KeyCollection : ICollection<TKey>
    {
        Dictionary<WeakReference, TValue> dictionary;
        public KeyCollection(Dictionary<WeakReference, TValue> Dictionary)
        {
            dictionary = Dictionary;
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => false;

        public void Add(TKey item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool Contains(TKey item)
        {
            return dictionary.ContainsKey(new WeakReference(item));
        }

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<TKey> GetEnumerator()
        {
            var keys = dictionary.Keys;
            foreach (var key in keys)
            {
                if (!key.IsAlive)
                {
                    dictionary.Remove(key);
                }
                else
                {
                    yield return (TKey)key.Target;
                }
            }
        }

        public bool Remove(TKey item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }



    public TValue this[TKey key] { get => dictionary[new WeakReference(key)]; set => dictionary[new WeakReference(key)] = value; }

    public ICollection<TKey> Keys => new KeyCollection(dictionary);

    public ICollection<TValue> Values => dictionary.Values;

    public int Count
    {
        get
        {
            DictionaryCheck();
            return dictionary.Count;
        }
    }

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        DictionaryCheck();
        dictionary.Add(new WeakReference(key), value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        DictionaryCheck();
        dictionary.Add(new WeakReference(item.Key), item.Value);
    }

    public void Clear()
    {
        dictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        DictionaryCheck();
        return dictionary.ContainsKey(new WeakReference(item.Key));
    }

    public bool ContainsKey(TKey key)
    {
        DictionaryCheck();
        return dictionary.ContainsKey(new WeakReference(key));
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        (dictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        var keys = dictionary.Keys;
        foreach (var key in keys)
        {
            if (!key.IsAlive)
            {
                dictionary.Remove(key);
            }
            else
            {
                yield return new KeyValuePair<TKey, TValue>((TKey)key.Target, dictionary[key]);
            }
        }
    }

    public bool Remove(TKey key)
    {
        DictionaryCheck();
        return dictionary.Remove(new WeakReference(key));
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        DictionaryCheck();
        return dictionary.Remove(new WeakReference(item.Key));
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        DictionaryCheck();
        return dictionary.TryGetValue(new WeakReference(key), out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        var keys = dictionary.Keys;
        foreach (var key in keys)
        {
            if (!key.IsAlive)
            {
                dictionary.Remove(key);
            }
            else
            {
                yield return dictionary[key];
            }
        }
    }

    void DictionaryCheck()
    {
        Modding.Logger.Log("OLD SIZE = " + dictionary.Count);
        var keys = dictionary.Keys;
        foreach (var key in keys)
        {
            if (!key.IsAlive)
            {
                dictionary.Remove(key);
            }
        }
        Modding.Logger.Log("NEW SIZE = " + dictionary.Count);
    }
}
*/