using System.Collections;
using System.Collections.Generic;

namespace Meep.Tech.Data {
  public static class DictionaryExtensions {

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
