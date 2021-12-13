﻿using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Meep.Tech.Data {

  public interface IReadableComponentStorage<Type> : IReadableComponentStorage {

    /// <summary>
    /// Internal holder for components data
    /// </summary>
    IReadOnlyDictionary<string, IComponent> ComponentsByBuilderKey
      => _componentsByBuilderKey;

    /// <summary>
    /// Internal holder for components data
    /// </summary>
    protected new Dictionary<string, IComponent> _componentsByBuilderKey {
      get;
    }

    /// <summary>
    /// Internal holder for components data
    /// </summary>
    Dictionary<string, IComponent> IReadableComponentStorage._componentsByBuilderKey
      => _componentsByBuilderKey;
  }

  /// <summary>
  /// This represents an object with some kind of component storage.
  /// To override component get logic on objects, you must override them implicitly via IReadableComponentStorage
  /// Overriding in the object itself without using implicits may not change the logic everywhere that's needed, 
  /// ... only do that if you want to add logic and use the base functionality too.
  /// </summary>
  public interface IReadableComponentStorage {

    /// <summary>
    /// Internal holder for components data
    /// </summary>
    internal Dictionary<string, IComponent> _componentsByBuilderKey {
      get;
    }

    /// <summary>
    /// Equality Logic
    /// TODO: impliment this in the WithComponents models
    /// </summary>
    public static bool Equals(IReadableComponentStorage model, IReadableComponentStorage other) {
      foreach((_, IComponent dataComponent) in model._componentsByBuilderKey) {
        // check each child component that we need to:
        if(Components.GetBuilderFactoryFor(dataComponent.GetType()).IncludeInParentModelEqualityChecks) {
          // if the other item doesn't have any components, is missing this component, or the other component doesn't equal the one from this model, it's not ==
          if(!other.HasComponent(dataComponent, out IComponent otherComponent)
            || !dataComponent.Equals(otherComponent)
          ) {
            return false;
          }
        }
      }

      return true;
    }

    /// <summary>
    /// Used to convert a collection of components to and from a json array
    /// </summary>
    public class ComponentsToJsonCollectionConverter : ValueConverter<Dictionary<string, IModel.IComponent>, string> {

      public ComponentsToJsonCollectionConverter() :
        base(convertToProviderExpression, convertFromProviderExpression) {
      }

      private static Expression<Func<string, Dictionary<string, IModel.IComponent>>> convertFromProviderExpression = x => FromJsonString(x);
      private static Expression<Func<Dictionary<string, IModel.IComponent>, string>> convertToProviderExpression = x => ToJsonString(x);

      static Dictionary<string, IModel.IComponent> FromJsonString(string componentsJson)
        => JArray.Parse(componentsJson).Select(token =>
          IComponent.FromJson(token as JObject)
        ).ToDictionary(
          component => component.Key,
          component => component
        );

      static string ToJsonString(Dictionary<string, IModel.IComponent> components)
        => JArray.FromObject(components.Select(componentData => componentData.Value.ToJson())).ToString();
    }

    #region Implicit Implementations

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public IComponent GetComponent(string key)
      => ReadableComponentStorageExtensions.GetComponent(this, key);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public TComponent GetComponent<TComponent>()
      where TComponent : IComponent<TComponent>
      => ReadableComponentStorageExtensions.GetComponent<TComponent>(this);

    /// <summary>
    /// Check if this has a component matching the given object.
    /// </summary>
    public bool HasComponent(string componentKey)
      => ReadableComponentStorageExtensions.HasComponent(this, componentKey);

    /// <summary>
    /// Get a component if this has that given component
    /// Overriding this overrides Get component and all other Has component functionalities
    /// </summary>
    public bool HasComponent(string componentKey, out IComponent component)
      => ReadableComponentStorageExtensions.HasComponent(this, componentKey, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public bool HasComponent(System.Type componentType, out IComponent component)
      => ReadableComponentStorageExtensions.HasComponent(this, componentType, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public bool HasComponent<TComponent>(out IComponent component)
      where TComponent : IComponent<TComponent>
        => ReadableComponentStorageExtensions.HasComponent<TComponent>(this, out component);

    /// <summary>
    /// Check if this has a given component by base type
    /// </summary>
    public bool HasComponent(System.Type componentType)
      => ReadableComponentStorageExtensions.HasComponent(this, componentType);

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public bool HasComponent(IComponent componentModel)
      => ReadableComponentStorageExtensions.HasComponent(this, componentModel);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public bool HasComponent(IComponent componentModel, out IComponent component)
      => ReadableComponentStorageExtensions.HasComponent(this, componentModel, out component);

    #endregion
  }

  /// <summary>
  /// This represens an object with component storage that you can write to without restrictions.
  /// Adding this to an object will give you unrestricted access to write to it's components
  /// These logics can't be replaced on objecs like models.
  /// If you want to add logic, you can instead override the virtual model function and use the base function logic.
  /// </summary>
  public interface IWriteableComponentStorage 
    : IReadableComponentStorage {}

  public static class ReadableComponentStorageExtensions {

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static IComponent GetComponent(this IReadableComponentStorage storage, string key)
      => storage.HasComponent(key, out IComponent component)
        ? component
        : throw new KeyNotFoundException($"No component of type {key} found in the storage.");

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static TComponent GetComponent<TComponent>(this IReadableComponentStorage storage)
      where TComponent : IComponent<TComponent>
        => (TComponent)storage.GetComponent(Components<TComponent>.Key);

    /// <summary>
    /// Check if this has a component matching the given object.
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, string componentKey)
      => storage.HasComponent(componentKey, out _);

    /// <summary>
    /// Get a component if this has that given component
    /// Overriding this overrides Get component and all other Has component functionalities
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, string componentBaseKey, out IComponent component)
      => storage._componentsByBuilderKey.TryGetValue(componentBaseKey, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, System.Type componentType, out IComponent component)
      => storage.HasComponent(Components.GetBuilderFactoryFor(componentType).Key, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static bool HasComponent<TComponent>(this IReadableComponentStorage storage, out IComponent component)
      where TComponent : IComponent<TComponent>
        => storage.HasComponent(Components<TComponent>.Key, out component);

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public static bool HasComponent<TComponent>(this IReadableComponentStorage storage)
      where TComponent : IComponent<TComponent>
        => storage.HasComponent(Components<TComponent>.Key);

    /// <summary>
    /// Check if this has a given component by base type
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, System.Type componentType)
      => storage.HasComponent(componentType, out _);

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, IComponent componentModel)
      => storage.HasComponent(componentModel.Key);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public static bool HasComponent(this IReadableComponentStorage storage, IComponent componentModel, out IComponent component)
      => storage.HasComponent(componentModel.Key, out component);

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// This is for internal use only
    /// </summary>
    internal static void AddComponent(this IReadableComponentStorage storage, IComponent toAdd) {
      storage._componentsByBuilderKey.Add(toAdd.Key, toAdd);
    }

    /// <summary>
    /// Add a component, updating the existing value if a component of this type already exists.
    /// </summary>
    internal static void AddOrUpdateComponent(this IReadableComponentStorage storage, IComponent toSet) {
      storage._componentsByBuilderKey[toSet.Key] = toSet;
    }

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    internal static void UpdateComponent(this IReadableComponentStorage storage, IComponent toUpdate) {
      if(storage.HasComponent(toUpdate.Key)) {
        storage._componentsByBuilderKey[toUpdate.Key] = toUpdate;
      }
      else
        throw new KeyNotFoundException($"Could not find compoennt of type {toUpdate.Key} to update.");
    }

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    internal static void UpdateComponent<TComponentType>(this IReadableComponentStorage storage, Func<TComponentType, TComponentType> UpdateComponent)
      where TComponentType : IComponent {
      if(storage.HasComponent(typeof(TComponentType), out IComponent current)) {
        storage._componentsByBuilderKey[current.Key] = UpdateComponent((TComponentType)current);
      }
      else
        throw new KeyNotFoundException($"Could not find compoennt with the key of type {typeof(TComponentType).FullName} to update.");
    }

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent<TComponent>(this IReadableComponentStorage storage)
      where TComponent : IComponent<TComponent>
        => storage.RemoveComponent(Components<TComponent>.Key);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent<TComponent>(this IReadableComponentStorage storage, out IComponent removedComponent)
      where TComponent : IComponent<TComponent>
        => storage.RemoveComponent(Components<TComponent>.Key, out removedComponent);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, System.Type toRemove)
      => storage.RemoveComponent(Components.GetBuilderFactoryFor(toRemove).Key);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, System.Type toRemove, out IComponent removedComponent)
      => storage.RemoveComponent(Components.GetBuilderFactoryFor(toRemove).Key, out removedComponent);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, IComponent toRemove)
      => storage.RemoveComponent(toRemove.Key);

    /// <summary>
    /// Basic remove component logic.
    /// This should be the only one you need to override for all removal logic
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, string componentKey, out IComponent removedComponent) {
      if(storage._componentsByBuilderKey.TryGetValue(componentKey, out removedComponent)) {
        storage._componentsByBuilderKey.Remove(componentKey);
        return true;
      }

      removedComponent = null;
      return false;
    }

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    internal static bool RemoveComponent(this IReadableComponentStorage storage, string componentKey)
      => storage.RemoveComponent(componentKey, out _);

  }

  public static class WriteableComponentStorageExtensions {

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    public static void AddComponent(this IWriteableComponentStorage storage, IComponent toAdd) 
      => ReadableComponentStorageExtensions.AddComponent(storage, toAdd);

    /// <summary>
    /// Add a component, updating the existing value if a component of this type already exists.
    /// </summary>
    public static void AddOrUpdateComponent(this IWriteableComponentStorage storage, IComponent toSet)
      => ReadableComponentStorageExtensions.AddOrUpdateComponent(storage, toSet);

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    public static void UpdateComponent(this IWriteableComponentStorage storage, IComponent toUpdate) 
      => ReadableComponentStorageExtensions.UpdateComponent(storage, toUpdate);

    /// <summary>
    /// Add a component, if it doesn't exist. Otherwise this throws.
    /// </summary>
    public static void UpdateComponent<TComponentType>(this IWriteableComponentStorage storage, Func<TComponentType, TComponentType> UpdateComponent)
      where TComponentType : IComponent
        => ReadableComponentStorageExtensions.UpdateComponent(storage, UpdateComponent);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, IComponent component)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, component);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, System.Type componentType)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, componentType);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, System.Type componentType, out IComponent foundComponent)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, componentType, out foundComponent);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent<TComponent>(this IWriteableComponentStorage storage)
      where TComponent : IComponent<TComponent>
        => ReadableComponentStorageExtensions.RemoveComponent<TComponent>(storage);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent<TComponent>(this IWriteableComponentStorage storage, out IComponent foundComponent)
      where TComponent : IComponent<TComponent>
        => ReadableComponentStorageExtensions.RemoveComponent<TComponent>(storage, out foundComponent);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, string componentKey, out IComponent removedComponent)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, componentKey, out removedComponent);

    /// <summary>
    /// Basic remove component logic
    /// </summary>
    public static bool RemoveComponent(this IWriteableComponentStorage storage, string componentKey)
        => ReadableComponentStorageExtensions.RemoveComponent(storage, componentKey);
  }
}