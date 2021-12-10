﻿using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Model<TModelBase>
    where TModelBase : IModel<TModelBase> 
  {

    /// <summary>
    /// A Model with Components built in
    /// </summary>
    public class WithComponents
      : Model<TModelBase>,
      IReadableComponentStorage 
    {

      /// <summary>
      /// Publicly readable components
      /// </summary>
      public IReadOnlyDictionary<string, Model.IComponent> components
        => _components.ToDictionary(x => x.Key, y => y.Value as Model.IComponent);

      /// <summary>
      /// The accessor for the default Icomponents implimentation
      /// </summary>
      Dictionary<string, Data.IComponent> IReadableComponentStorage._componentsByBuilderKey
        => _components;

      /// <summary>
      /// Internally stored components
      /// </summary>
      [IsModelComponentsProperty]
      Dictionary<string, Data.IComponent> _components {
        get;
      }


      #region Default Component Implimentations

      #region Read

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent(string componentKey)
        => (this as IReadableComponentStorage).GetComponent(componentKey) as Model.IComponent;

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent<TComponent>(string componentKey)
        where TComponent : Model.IComponent
          => (this as IReadableComponentStorage).GetComponent(componentKey) as Model.IComponent;

      /// <summary>
      /// Get a component if this has a component of that given type
      /// </summary>
      public virtual bool HasComponent(System.Type componentType, out Model.IComponent component) {
        if((this as IReadableComponentStorage).HasComponent(componentType, out Data.IComponent found)) {
          component = found as Model.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a given component by base type
      /// TODO, if this causes a stackoverflow we'll need to use the extensionmethod trick... which may be cleaner anyway lol
      /// </summary>
      public virtual bool HasComponent(System.Type componentType)
        => (this as IReadableComponentStorage).HasComponent(componentType);

      /// <summary>
      /// Check if this has a component matching the given object
      /// TODO, if this causes a stackoverflow we'll need to use the extensionmethod trick... which may be cleaner anyway lol
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey)
        => (this as IReadableComponentStorage).HasComponent(componentBaseKey);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey, out Model.IComponent component) {
        if((this as IReadableComponentStorage).HasComponent(componentBaseKey, out Data.IComponent found)) {
          component = found as Model.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(Model.IComponent componentModel)
        => (this as IReadableComponentStorage).HasComponent(componentModel);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool HasComponent(Model.IComponent componentModel, out Model.IComponent component) {
        if((this as IReadableComponentStorage).HasComponent(componentModel, out Data.IComponent found)) {
          component = found as Model.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      #endregion

      #region Write

      /// <summary>
      /// Add a new component, throws if the component key is taken already
      /// </summary>
      protected virtual void AddComponent(Model.IComponent toAdd) {
        if(toAdd is IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }

        (this as IReadableComponentStorage).AddComponent(toAdd);
      }

      /// <summary>
      /// replace an existing component
      /// </summary>
      protected virtual void UpdateComponent(Model.IComponent toUpdate) {
        (this as IReadableComponentStorage).UpdateComponent(toUpdate);
      }

      /// <summary>
      /// update an existing component, given it's current data
      /// </summary>
      protected virtual void UpdateComponent<TComponent>(System.Func<TComponent, TComponent> UpdateComponent)
        where TComponent : Model.IComponent {
        (this as IReadableComponentStorage).UpdateComponent(UpdateComponent);
      }

      /// <summary>
      /// Add or replace a component
      /// </summary>
      protected virtual void AddOrUpdateComponent(Model.IComponent toSet) {
        if(toSet is IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toSet.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }
        (this as IReadableComponentStorage).AddOrUpdateComponent(toSet);
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(Model.IComponent toRemove)
        => (this as IReadableComponentStorage).RemoveComponent(toRemove.Key);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>()
        where TComponent : Model.IComponent<TComponent>
          => (this as IReadableComponentStorage).RemoveComponent<TComponent>();

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>(out IComponent removed)
        where TComponent : Model.IComponent<TComponent> {
        if((this as IReadableComponentStorage).RemoveComponent<TComponent>(out Data.IComponent found)) {
          removed = found as Model.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove)
        => (this as IReadableComponentStorage).RemoveComponent(toRemove);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove, out IComponent removed) {
        if((this as IReadableComponentStorage).RemoveComponent(toRemove, out Data.IComponent found)) {
          removed = found as Model.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove and get an existing component
      /// </summary>
      protected virtual bool RemoveComponent(string componentKeyToRemove, out Model.IComponent removedComponent) {
        if((this as IReadableComponentStorage).RemoveComponent(componentKeyToRemove, out Data.IComponent component)) {
          removedComponent = component as Model.IComponent;
          return true;
        }

        removedComponent = null;
        return false;
      }

      #endregion

      #endregion
    }
  }

  public partial class Model<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase, TArchetypeBase> 
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {
    /// <summary>
    /// A Model with Components built in
    /// </summary>
    public class WithComponents
      : Model<TModelBase, TArchetypeBase>,
      IReadableComponentStorage 
    {

      /// <summary>
      /// Publicly readable components
      /// </summary>
      public IReadOnlyDictionary<string, Model.IComponent> components
        => _components.ToDictionary(x => x.Key, y => y.Value as Model.IComponent);

      /// <summary>
      /// The accessor for the default Icomponents implimentation
      /// </summary>
      Dictionary<string, Data.IComponent> IReadableComponentStorage._componentsByBuilderKey
        => _components;

      /// <summary>
      /// Internally stored components
      /// </summary>
      [IsModelComponentsProperty]
      Dictionary<string, Data.IComponent> _components {
        get;
      }

      #region Default Component Implimentations

      #region Read

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent(string componentKey)
        => (this as IReadableComponentStorage).GetComponent(componentKey) as Model.IComponent;

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent<TComponent>(string componentKey)
        where TComponent : Model.IComponent
          => (this as IReadableComponentStorage).GetComponent(componentKey) as Model.IComponent;

      /// <summary>
      /// Get a component if this has a component of that given type
      /// </summary>
      public virtual bool HasComponent(System.Type componentType, out Model.IComponent component) {
        if((this as IReadableComponentStorage).HasComponent(componentType, out Data.IComponent found)) {
          component = found as Model.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a given component by base type
      /// TODO, if this causes a stackoverflow we'll need to use the extensionmethod trick... which may be cleaner anyway lol
      /// </summary>
      public virtual bool HasComponent(System.Type componentType)
        => (this as IReadableComponentStorage).HasComponent(componentType);

      /// <summary>
      /// Check if this has a component matching the given object
      /// TODO, if this causes a stackoverflow we'll need to use the extensionmethod trick... which may be cleaner anyway lol
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey)
        => (this as IReadableComponentStorage).HasComponent(componentBaseKey);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey, out Model.IComponent component) {
        if((this as IReadableComponentStorage).HasComponent(componentBaseKey, out Data.IComponent found)) {
          component = found as Model.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(Model.IComponent componentModel)
        => (this as IReadableComponentStorage).HasComponent(componentModel);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool HasComponent(Model.IComponent componentModel, out Model.IComponent component) {
        if((this as IReadableComponentStorage).HasComponent(componentModel, out Data.IComponent found)) {
          component = found as Model.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      #endregion

      #region Write

      /// <summary>
      /// Add a new component, throws if the component key is taken already
      /// </summary>
      protected virtual void AddComponent(Model.IComponent toAdd) {
        if(toAdd is IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }

        (this as IReadableComponentStorage).AddComponent(toAdd);
      }

      /// <summary>
      /// replace an existing component
      /// </summary>
      protected virtual void UpdateComponent(Model.IComponent toUpdate) {
        (this as IReadableComponentStorage).UpdateComponent(toUpdate);
      }

      /// <summary>
      /// update an existing component, given it's current data
      /// </summary>
      protected virtual void UpdateComponent<TComponent>(System.Func<TComponent, TComponent> UpdateComponent)
        where TComponent : Model.IComponent {
        (this as IReadableComponentStorage).UpdateComponent(UpdateComponent);
      }

      /// <summary>
      /// Add or replace a component
      /// </summary>
      protected virtual void AddOrUpdateComponent(Model.IComponent toSet) {
        if(toSet is IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toSet.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }
        (this as IReadableComponentStorage).AddOrUpdateComponent(toSet);
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(Model.IComponent toRemove)
        => (this as IReadableComponentStorage).RemoveComponent(toRemove.Key);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>()
        where TComponent : Model.IComponent<TComponent>
          => (this as IReadableComponentStorage).RemoveComponent<TComponent>();

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>(out IComponent removed)
        where TComponent : Model.IComponent<TComponent> {
        if((this as IReadableComponentStorage).RemoveComponent<TComponent>(out Data.IComponent found)) {
          removed = found as Model.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove)
        => (this as IReadableComponentStorage).RemoveComponent(toRemove);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove, out IComponent removed) {
        if((this as IReadableComponentStorage).RemoveComponent(toRemove, out Data.IComponent found)) {
          removed = found as Model.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove and get an existing component
      /// </summary>
      protected virtual bool RemoveComponent(string componentKeyToRemove, out Model.IComponent removedComponent) {
        if((this as IReadableComponentStorage).RemoveComponent(componentKeyToRemove, out Data.IComponent component)) {
          removedComponent = component as Model.IComponent;
          return true;
        }

        removedComponent = null;
        return false;
      }

      #endregion

      #endregion
    }
  }
}