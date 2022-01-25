using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;


namespace Meep.Tech.Data.Utility {
  public class OrderedDictionary<TKey, TValue> 
    : IEnumerable<KeyValuePair<TKey, TValue>> 
  {

    public OrderedDictionary _collection { 
      get;
    } = new OrderedDictionary();

    public TValue this[TKey key] {
      get => (TValue)_collection[key];
      set => _collection[key] = value;
    }

    public TValue this[int index] {
      get => (TValue)_collection[index];
      set => _collection[index] = value;
    }

    public ICollection<TKey> Keys 
      => _collection.Keys.OfType<TKey>().ToList();

    public ICollection<TValue> Values 
      => _collection.Values.OfType<TValue>().ToList();

    public bool IsReadOnly 
      => _collection.IsReadOnly;

    public int Count 
      => _collection.Count;

    public virtual void Insert(int index, TKey key, TValue value) 
      => _collection.Insert(index, key, value);

    public virtual void RemoveAt(int index) 
      => _collection.RemoveAt(index);

    public bool Contains(TKey key) 
      => _collection.Contains(key);

    public virtual void Add(TKey key, TValue value) 
      => _collection.Add(key, value);

    public virtual void Clear() 
      => _collection.Clear();

    public virtual void Remove(TKey key) 
      => _collection.Remove(key);

    public void CopyTo(Array array, int index) 
      => _collection.CopyTo(array, index);

    public bool TryGetValue(TKey key, out TValue value)
      => (value = Contains(key)
        ? this[key]
        : default
      ) != null;

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
      => _collection.Cast<DictionaryEntry>().Select(obj => new KeyValuePair<TKey, TValue>(
        (TKey)(obj).Key,
        (TValue)(obj).Value
      )).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
      => _collection.Cast<DictionaryEntry>().Select(obj => new KeyValuePair<TKey, TValue>(
        (TKey)(obj).Key,
        (TValue)(obj).Value
      )).GetEnumerator();
  }
}