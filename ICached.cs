using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// Base functionality for cacheable models
  /// </summary>
  public interface ICached : IUnique {

    internal static void Cache(ICached thingToCache) {
      _cache.Add(thingToCache.id, thingToCache);
    }

    internal static Dictionary<string, IUnique> _cache
      = new Dictionary<string, IUnique>();

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static IUnique FromCache(string modelId) 
      => _cache.TryGetValue(modelId, out IUnique fetchedModel)
        ? fetchedModel
        : null;
  }

  /// <summary>
  /// A Model that can be cached
  /// </summary>
  public interface ICached<T> : ICached
    where T : class, ICached<T>
  {

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static new T FromCache(string modelId) {
      IUnique fetched = null;
      try { 
        return (fetched = ICached.FromCache(modelId)) as T; 
      } catch (InvalidCastException e) {
        throw new InvalidCastException($"Fetched Model From Cache with ID {modelId} is likely not of type {typeof(T).FullName}. Actual type: {fetched?.GetType().FullName ?? "NULL"}", e);
      };
    }

    public static void Cache(T thingToCache) {
      _cache[thingToCache.id] = thingToCache;
    }

    void IModel.finishDeserialization() {
      Cache((T)this);
    }
  }
}