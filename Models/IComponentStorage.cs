using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  internal interface IComponentStorage {

    /// <summary>
    /// Internal holder for components data
    /// </summary>
    internal Dictionary<string, IComponent> _componentsByBuilderKey {
      get;
    }

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public virtual bool hasComponent(System.Type componentType, out IComponent component) 
      => _componentsByBuilderKey.TryGetValue(Components.GetBuilderFactoryFor(componentType).Key, out component);

    /// <summary>
    /// Check if this has a given component by base type
    /// </summary>
    public bool hasComponent(System.Type componentType)
      => _componentsByBuilderKey.ContainsKey(Components.GetBuilderFactoryFor(componentType).Key);

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public bool hasComponent(string componentBaseKey)
      => _componentsByBuilderKey.ContainsKey(componentBaseKey);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public virtual bool hasComponent(string componentBaseKey, out IComponent component) 
      => _componentsByBuilderKey.TryGetValue(componentBaseKey, out component);

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public bool hasComponent(object componentModel)
      => hasComponent((componentModel as IComponent).Key);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public virtual bool hasComponent(object componentModel, out IComponent component) 
      => hasComponent((componentModel as IComponent).Key, out component);

    /// <summary>
    /// Equality Logic
    /// </summary>
    public static bool Equals(IComponentStorage model, IComponentStorage other) {
      foreach((_, IComponent dataComponent) in model._componentsByBuilderKey) {
        // check each child component that we need to:
        if(Components.GetBuilderFactoryFor(dataComponent.GetType()).IncludeInParentModelEqualityChecks) {
          // if the other item doesn't have any components, is missing this component, or the other component doesn't equal the one from this model, it's not ==
          if(!other.hasComponent(dataComponent, out IComponent otherComponent)
            || !dataComponent.Equals(otherComponent)
          ) {
            return false;
          }
        }
      }

      return true;
    }
  }
}