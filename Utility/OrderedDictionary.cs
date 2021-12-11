using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;


namespace Meep.Tech.Data.Utility {
  public class OrderdDictionary<K, V> : IEnumerable<KeyValuePair<K, V>> {
    public OrderedDictionary UnderlyingCollection { get; } = new OrderedDictionary();

    public V this[K key] {
      get {
        return (V)UnderlyingCollection[key];
      }
      set {
        UnderlyingCollection[key] = value;
      }
    }

    public V this[int index] {
      get {
        return (V)UnderlyingCollection[index];
      }
      set {
        UnderlyingCollection[index] = value;
      }
    }
    public ICollection<K> Keys => UnderlyingCollection.Keys.OfType<K>().ToList();
    public ICollection<V> Values => UnderlyingCollection.Values.OfType<V>().ToList();
    public bool IsReadOnly => UnderlyingCollection.IsReadOnly;
    public int Count => UnderlyingCollection.Count;
    public void Insert(int index, K key, V value) => UnderlyingCollection.Insert(index, key, value);
    public void RemoveAt(int index) => UnderlyingCollection.RemoveAt(index);
    public bool Contains(K key) => UnderlyingCollection.Contains(key);
    public void Add(K key, V value) => UnderlyingCollection.Add(key, value);
    public void Clear() => UnderlyingCollection.Clear();
    public void Remove(K key) => UnderlyingCollection.Remove(key);
    public void CopyTo(Array array, int index) => UnderlyingCollection.CopyTo(array, index);

    /// <summary>
    /// Try get for ordered dic
    /// </summary>
    public bool TryGetValue(K key, out V value)
      => (value = Contains(key)
        ? this[key]
        : default
      ) != null;

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
      => UnderlyingCollection.Cast<DictionaryEntry>().Select(obj => new KeyValuePair<K, V>(
        (K)(obj).Key,
        (V)(obj).Value
      )).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
      => UnderlyingCollection.Cast<DictionaryEntry>().Select(obj => new KeyValuePair<K, V>(
        (K)(obj).Key,
        (V)(obj).Value
      )).GetEnumerator();
  }
}