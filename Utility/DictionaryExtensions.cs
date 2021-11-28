using System;
using System.Collections;
using System.Collections.Generic;

namespace Meep.Tech.Data {
  public static class DictionaryExtensions {

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
