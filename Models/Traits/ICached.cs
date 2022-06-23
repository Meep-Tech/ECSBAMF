using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// Base functionality for cacheable models
  /// </summary>
  public interface ICached : IUnique {

    internal static Dictionary<string, IUnique> _cache
      = new();

    internal static void _cacheItem(ICached thingToCache) 
      => _cache.Add(thingToCache.Id, thingToCache);

    /// <summary>
    /// Try to load an item fro mthe cache by id.
    /// </summary>
    public static IUnique TryToGetFromCache(string modelId) 
      => _cache.TryGetValue(modelId, out IUnique fetchedModel)
        ? fetchedModel
        : null;

    /// <summary>
    ///  load an item from the cache by id.
    /// </summary>
    public static IUnique GetFromCache(string modelId)
      => _cache[modelId];
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
    public static new T TryToGetFromCache(string modelId) {
      IUnique fetched = null;
      try { 
        return (fetched = ICached.TryToGetFromCache(modelId)) as T; 
      } catch (InvalidCastException e) {
        throw new InvalidCastException($"Fetched Model From Cache with ID {modelId} is likely not of type {typeof(T).FullName}. Actual type: {fetched?.GetType().FullName ?? "NULL"}", e);
      };
    }

    /// <summary>
    /// Load an item from the cache by id.
    /// </summary>
    public new static T GetFromCache(string modelId) 
      => (T)ICached.GetFromCache(modelId);

    /// <summary>
    /// Try to load an item from the cache by id
    /// </summary>
    public static bool TryToGetFromCache(string modelId, out T found)
      => (found = TryToGetFromCache(modelId)) != null;

    /// <summary>
    /// Cache an item of the given type.
    /// </summary>
    public static void Cache(T thingToCache) {
      _cache[thingToCache.Id] = thingToCache;
    }

    void IModel.FinishDeserialization() {
      Cache((T)this);
    }
  }
}