using System;
using System.Collections.Generic;


namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A collection of items, each indexed by a key and in list order by an index.
  /// Read + Writeable
  /// </summary>
  public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyOrderedDictionary<TKey, TValue> {

    /// <summary>
    /// Sets the value at the index specified, maintaining the same key that's currently pointing at this index.
    /// </summary>
    void SetByIndex(int index, TValue value);

    /// <summary>
    /// set a value by it's key, adding a new entry if the key isn't present at the end of the list.
    /// </summary>
    void SetByKey(TKey key, TValue value);

    /// <summary>
    /// Insert an item into the collection at an existing index.
    /// </summary>
    void InsertByIndex(int index, TKey key, TValue value);

    /// <summary>
    /// Remove the value at the given index.
    /// </summary>
    void RemoveAt(int index);

    ///<summary>
    /// Remove using the key.
    /// This can help as an alternative to Remove if the key is an int
    /// </summary>
    bool RemoveByKey(TKey key);
  }

  /// <summary>
  /// A Read only ordered dictionary.
  /// </summary>
  public interface IReadOnlyOrderedDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>> {

    /// <summary>
    /// Get the item at the given ordered index.
    /// </summary>
    TValue this[int index] { get; set; }

    /// <summary>
    /// Get the item at the given ordered index.
    /// </summary>
    TValue GetValueAtIndex(int index);

    /// <summary>
    /// Get the key at the given index
    /// </summary>
    TKey GetKeyAtIndex(int index);

    /// <summary>
    /// Get the item at the given ordered index.
    /// </summary>
    KeyValuePair<TKey, TValue> GetPairAtIndex(int index);

    /// <summary>
    /// Try to get the item at the given ordered index.
    /// </summary>
    /// <param name="value">The found value, or default if not found.</param>
    bool TryToGetPairAtIndex(int index, out KeyValuePair<TKey, TValue> value);

    /// <summary>
    /// Get the item at the given ordered index.
    /// </summary>
    /// <returns>The value, or Default if not found.</returns>
    KeyValuePair<TKey, TValue> TryToGetPairAtIndex(int index);

    /// <summary>
    /// Get the item with the given key.
    /// </summary>
    TValue GetValueWithKey(TKey index);

    /// <summary>
    /// Try to get the item at the given ordered index.
    /// </summary>
    /// <param name="value">The found value, or default if not found.</param>
    bool TryGetValueAtIndex(int index, out TValue value);

    /// <summary>
    /// Try to get the item with the given key.
    /// </summary>
    /// <returns>The value, or Default if not found.</returns>
    TValue TryGetValue(TKey key);

    /// <summary>
    /// Try to get the item at the given ordered index.
    /// </summary>
    /// <returns>The value, or Default if not found.</returns>
    TValue TryGetValueAtIndex(int index);

    /// <summary>
    /// Check if this contains both the key and value pair.
    /// </summary>
    bool Contains(KeyValuePair<TKey, TValue> pair);

    /// <summary>
    /// Check if this contains the value
    /// </summary>
    bool ContainsValue(TValue item);

    /// <summary>
    /// Get the index of the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    int IndexOf(TKey key);

    ///<summary>
    /// Try to get the key at the given index.
    /// </summary>
    /// <returns>The key, or Default if not found.</returns>
    TKey TryGetKeyAtIndex(int index);

    ///<summary>
    /// Try to get the key at the given index.
    /// </summary>
    /// <param name="value">The found key, or default if not found.</param>
    bool TryGetKeyAtIndex(int index, out TKey value);
  }
}