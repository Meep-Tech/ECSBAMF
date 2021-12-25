using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {
  public static class DictionaryExtensions {

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

    #endregion

    #region  Append

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static HashSet<TValue> Append<TValue>(this HashSet<TValue> current, TValue value) {
      current.Add(value);
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
    /// Append a component to a dictionary and return the collection
    /// </summary>
    public static Dictionary<string, TComponentBase> Append<TComponentBase>(this Dictionary<string, TComponentBase> current, TComponentBase component)
      where TComponentBase : IComponent
    {
      current.Add(component.Key, component);
      return current;
    }

    #endregion

    #region Merge 

    /// <summary>
    /// Merge dictionaries together, overriding any values with the same key.
    /// </summary>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> baseDict, params Dictionary<TKey, TValue>[] dictionariesToMergeIn) {
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
