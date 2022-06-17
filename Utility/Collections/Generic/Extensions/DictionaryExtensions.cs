using Meep.Tech.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {
  public static class DictionaryExtensions {

    /// <summary>
    /// Try tho get a value. Returns default on failure.
    /// </summary>
    public static TValue TryToGet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
      => dictionary.TryGetValue(key, out var found) ? found : default;

    /// <summary>
    /// Try tho get a value. Returns default on failure.
    /// </summary>
    public static TValue TryToGet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue @default)
      => dictionary.TryGetValue(key, out var found) ? found : (dictionary[key] = @default);

    /// <summary>
    /// Add an item inline without needing to make it if it contains it's own key
    /// </summary>
    public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, Func<TValue, TKey> getKey)
      => dictionary.Add(getKey(value), value);

    /// <summary>
    /// Set ([]=) alias.
    /// </summary>
    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) {
      dictionary[key] = value;
    }

    #region ValueCollectionManipulation

        /// <summary>
        /// Add an item to a ICollection within a dictionary at the given key
        /// </summary>
        public static void AddToValueCollection<TKey, TValue>(this IDictionary<TKey, ICollection<TValue>> dictionary, TKey key, TValue value) {
      if(dictionary.TryGetValue(key, out ICollection<TValue> valueCollection)) {
        valueCollection.Add(value);
      }
      else
        dictionary.Add(key, new List<TValue> { value });
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AppendToValueCollection<TKey, TValue>(this IDictionary<TKey, IEnumerable<TValue>> dictionary, TKey key, TValue value) {
      if(dictionary.TryGetValue(key, out IEnumerable<TValue> currentValueCollection)) {
       dictionary[key] = currentValueCollection.Append(value);
      }
      else
        dictionary.Add(key, new List<TValue> { value });
    }

    /// <summary>
    /// Remove an item from an ICollection within a dictionary at the given key
    /// </summary>
    public static bool RemoveFromValueCollection<TKey, TValue>(this IDictionary<TKey, ICollection<TValue>> dictionary, TKey key, TValue value) {
      if(dictionary.TryGetValue(key, out ICollection<TValue> valueCollection)) {
        if(valueCollection.Remove(value)) {
          if(!valueCollection.Any()) {
            dictionary.Remove(key);
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AddToValueCollection<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictionary, TKey key, TValue value) {
      if(dictionary.TryGetValue(key, out IList<TValue> valueCollection)) {
        valueCollection.Add(value);
      }
      else
        dictionary.Add(key, new List<TValue> { value });
    }

    /// <summary>
    /// Remove an item from an ICollection within a dictionary at the given key
    /// </summary>
    public static bool RemoveFromValueCollection<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictionary, TKey key, TValue value) {
      if(dictionary.TryGetValue(key, out IList<TValue> valueCollection)) {
        if(valueCollection.Remove(value)) {
          if(!valueCollection.Any()) {
            dictionary.Remove(key);
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static void AddToHashSet<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out HashSet<TValue> valueCollection)) {
        valueCollection.Add(value);
      } else
        dictionary.Add(key, new HashSet<TValue> { value });
    }

    /// <summary>
    /// Add an item to a ICollection within a dictionary at the given key
    /// </summary>
    public static bool RemoveFromHashSet<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value) {
      if (dictionary.TryGetValue(key, out HashSet<TValue> valueCollection)) {
        return valueCollection.Remove(value);
      } else return false;
    }

    /// <summary>
    /// Bucketize a collecton of keys and values.
    /// </summary>
    public static Dictionary<TKey, ICollection<TValue>> Bucketize<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items) {
      Dictionary<TKey, ICollection<TValue>> @return = new();
      foreach (var item in items) {
        @return.AddToValueCollection(item.Key, item.Value);
      }

      return @return;
    }

    #endregion

    #region Append

    /// <summary>
    /// Append a value to a hash set and return the collection
    /// </summary>
    public static HashSet<TValue> Append<TValue>(this HashSet<TValue> current, TValue value) {
      current.Add(value);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static IDictionary<TKey, TValue> Append<TKey, TValue>(this IDictionary<TKey, TValue> current, TKey key, TValue value) {
      current.Add(key, value);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static Dictionary<TKey, TValue> Append<TKey, TValue>(this Dictionary<TKey, TValue> current, TKey key, TValue value) {
      current.Add(key, value);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static DelegateCollection<TAction> Append<TAction>(this DelegateCollection<TAction> current, string key, TAction action)
      where TAction : Delegate  
    {
      current.Add(key, action);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static OrderedDictionary<TKey, TValue> Append<TKey, TValue>(this OrderedDictionary<TKey, TValue> current, TKey key, TValue value) {
      current.Add(key, value);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static TDictionary Append<TDictionary, TKey, TValue>(this TDictionary current, TKey key, TValue value) 
      where TDictionary : IDictionary<TKey, TValue> 
    {
      current.Add(key, value);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static IDictionary<TKey, TValue> AppendOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> current, TKey key, TValue value) {
      current[key] = value;
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static Dictionary<TKey, TValue> AppendOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> current, TKey key, TValue value) {
      current[key] = value;
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static DelegateCollection<TAction> AppendOrReplace<TAction>(this DelegateCollection<TAction> current, string key, TAction action)
      where TAction : Delegate {
      current[key] = action;
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static OrderedDictionary<TKey, TValue> AppendOrReplace<TKey, TValue>(this OrderedDictionary<TKey, TValue> current, TKey key, TValue value) {
      current[key] = value;
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static TDictionary AppendOrReplace<TDictionary, TKey, TValue>(this TDictionary current, TKey key, TValue value) 
      where TDictionary : IDictionary<TKey, TValue> {
      current[key] = value;
      return current;
    }

    #endregion

    #region Merge 

    /// <summary>
    /// Merge dictionaries together, overriding any values with the same key.
    /// </summary>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> baseDict, params IDictionary<TKey, TValue>[] dictionariesToMergeIn) {
      if(dictionariesToMergeIn == null)
        return baseDict;
      Dictionary<TKey, TValue> result = baseDict;
      foreach(Dictionary<TKey, TValue> dictionary in dictionariesToMergeIn) {
        foreach(KeyValuePair<TKey, TValue> entry in dictionary) {
          result[entry.Key] = entry.Value;
        }
      }

      return result;
    }

    /// <summary>
    /// Merge dictionaries together, overriding any values with the same key.
    /// </summary>
    public static Dictionary<TKey, TValue> MergeInReadonly<TKey, TValue>(this Dictionary<TKey, TValue> baseDict, params IReadOnlyDictionary<TKey, TValue>[] dictionariesToMergeIn) {
      if(dictionariesToMergeIn == null)
        return baseDict;
      Dictionary<TKey, TValue> result = baseDict;
      foreach(Dictionary<TKey, TValue> dictionary in dictionariesToMergeIn) {
        foreach(KeyValuePair<TKey, TValue> entry in dictionary) {
          result[entry.Key] = entry.Value;
        }
      }

      return result;
    }

    /// <summary>
    /// Merge dictionaries together, overriding any values with the same key.
    /// </summary>
    public static IDictionary Merge(this IDictionary baseDict, params IDictionary[] dictionariesToMergeIn) {
      if(dictionariesToMergeIn == null)
        return baseDict;
      IDictionary result = baseDict;
      foreach(IDictionary dictionary in dictionariesToMergeIn) {
        foreach(object key in dictionary.Keys) {
          result[key] = dictionary[key];
        }
      }
      return result;
    }

    #endregion
  }
}
