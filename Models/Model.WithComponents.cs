using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public abstract partial class Model<TModelBase>
    where TModelBase : Model<TModelBase> 
  {

    /// <summary>
    /// A Model with Components built in
    /// </summary>
    public abstract class WithComponents
      : Model<TModelBase>,
      IReadableComponentStorage 
    {

      /// <summary>
      /// Publicly readable components
      /// </summary>
      public IReadOnlyDictionary<string, IModel.IComponent> components
        => _components.ToDictionary(x => x.Key, y => y.Value as IModel.IComponent);

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
      } = new Dictionary<string, Data.IComponent>();


      #region Default Component Implimentations

      #region Read

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent(string componentKey)
        => ReadableComponentStorageExtensions.GetComponent(this, componentKey) as IModel.IComponent;

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent<TComponent>(string componentKey)
        where TComponent : IModel.IComponent
          => ReadableComponentStorageExtensions.GetComponent(this, componentKey) as IModel.IComponent;

      /// <summary>
      /// Get a component if this has a component of that given type
      /// </summary>
      public virtual bool HasComponent(System.Type componentType, out IModel.IComponent component) {
        if(ReadableComponentStorageExtensions.HasComponent(this, componentType, out Data.IComponent found)) {
          component = found as IModel.IComponent;
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
        => ReadableComponentStorageExtensions.HasComponent(this, componentType);

      /// <summary>
      /// Check if this has a component matching the given object
      /// TODO, if this causes a stackoverflow we'll need to use the extensionmethod trick... which may be cleaner anyway lol
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey)
        => ReadableComponentStorageExtensions.HasComponent(this, componentBaseKey);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey, out IModel.IComponent component) {
        if(ReadableComponentStorageExtensions.HasComponent(this, componentBaseKey, out Data.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(IModel.IComponent componentModel)
        => ReadableComponentStorageExtensions.HasComponent(this, componentModel);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool HasComponent(IModel.IComponent componentModel, out IModel.IComponent component) {
        if(ReadableComponentStorageExtensions.HasComponent(this, componentModel, out Data.IComponent found)) {
          component = found as IModel.IComponent;
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
      protected virtual void AddComponent(IModel.IComponent toAdd) {
        if(toAdd is IModel.IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }

        ReadableComponentStorageExtensions.AddComponent(this, toAdd);
      }

      /// <summary>
      /// replace an existing component
      /// </summary>
      protected virtual void UpdateComponent(IModel.IComponent toUpdate) {
        ReadableComponentStorageExtensions.UpdateComponent(this, toUpdate);
      }

      /// <summary>
      /// update an existing component, given it's current data
      /// </summary>
      protected virtual void UpdateComponent<TComponent>(System.Func<TComponent, TComponent> UpdateComponent)
        where TComponent : IModel.IComponent {
        ReadableComponentStorageExtensions.UpdateComponent(this, UpdateComponent);
      }

      /// <summary>
      /// Add or replace a component
      /// </summary>
      protected virtual void AddOrUpdateComponent(IModel.IComponent toSet) {
        if(toSet is IModel.IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toSet.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }
        ReadableComponentStorageExtensions.AddOrUpdateComponent(this, toSet);
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(IModel.IComponent toRemove)
        => ReadableComponentStorageExtensions.RemoveComponent(this, toRemove.Key);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>()
        where TComponent : IModel.IComponent<TComponent>
          => ReadableComponentStorageExtensions.RemoveComponent<TComponent>(this);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>(out IComponent removed)
        where TComponent : IModel.IComponent<TComponent> {
        if(ReadableComponentStorageExtensions.RemoveComponent<TComponent>(this, out Data.IComponent found)) {
          removed = found as IModel.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove)
        => ReadableComponentStorageExtensions.RemoveComponent(this, toRemove);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove, out IComponent removed) {
        if(ReadableComponentStorageExtensions.RemoveComponent(this, toRemove, out Data.IComponent found)) {
          removed = found as IModel.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove and get an existing component
      /// </summary>
      protected virtual bool RemoveComponent(string componentKeyToRemove, out IModel.IComponent removedComponent) {
        if(ReadableComponentStorageExtensions.RemoveComponent(this, componentKeyToRemove, out Data.IComponent component)) {
          removedComponent = component as IModel.IComponent;
          return true;
        }

        removedComponent = null;
        return false;
      }

      #endregion

      #endregion
    }
  }

  public abstract partial class Model<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase, TArchetypeBase> 
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {
    /// <summary>
    /// A Model with Components built in
    /// </summary>
    public abstract class WithComponents
      : Model<TModelBase, TArchetypeBase>,
      IReadableComponentStorage 
    {

      /// <summary>
      /// Publicly readable components
      /// </summary>
      public IReadOnlyDictionary<string, IModel.IComponent> components
        => _components.ToDictionary(x => x.Key, y => y.Value as IModel.IComponent);

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
      } = new Dictionary<string, IComponent>();

      #region Default Component Implimentations

      #region Read

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent(string componentKey)
        => ReadableComponentStorageExtensions.GetComponent(this, componentKey) as IModel.IComponent;

      /// <summary>
      /// Get a component if it exists. Throws if it doesn't
      /// </summary>
      public virtual IComponent GetComponent<TComponent>(string componentKey)
        where TComponent : IModel.IComponent
          => ReadableComponentStorageExtensions.GetComponent(this, componentKey) as IModel.IComponent;

      /// <summary>
      /// Get a component if this has a component of that given type
      /// </summary>
      public virtual bool HasComponent(System.Type componentType, out IModel.IComponent component) {
        if(ReadableComponentStorageExtensions.HasComponent(this, componentType, out Data.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a given component by base type
      /// </summary>
      public virtual bool HasComponent(System.Type componentType)
        => ReadableComponentStorageExtensions.HasComponent(this, componentType);

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey)
        => ReadableComponentStorageExtensions.HasComponent(this, componentBaseKey);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool HasComponent(string componentBaseKey, out IModel.IComponent component) {
        if(ReadableComponentStorageExtensions.HasComponent(this, componentBaseKey, out Data.IComponent found)) {
          component = found as IModel.IComponent;
          return true;
        }

        component = null;
        return false;
      }

      /// <summary>
      /// Check if this has a component matching the given object
      /// </summary>
      public virtual bool HasComponent(IModel.IComponent componentModel)
        => ReadableComponentStorageExtensions.HasComponent(this, componentModel);

      /// <summary>
      /// Get a component if this has that given component
      /// </summary>
      public virtual bool HasComponent(IModel.IComponent componentModel, out IModel.IComponent component) {
        if(ReadableComponentStorageExtensions.HasComponent(this, componentModel, out Data.IComponent found)) {
          component = found as IModel.IComponent;
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
      protected virtual void AddComponent(IModel.IComponent toAdd) {
        if(toAdd is IModel.IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }

        ReadableComponentStorageExtensions.AddComponent(this, toAdd);
      }

      /// <summary>
      /// replace an existing component
      /// </summary>
      protected virtual void UpdateComponent(IModel.IComponent toUpdate) {
        ReadableComponentStorageExtensions.UpdateComponent(this, toUpdate);
      }

      /// <summary>
      /// update an existing component, given it's current data
      /// </summary>
      protected virtual void UpdateComponent<TComponent>(System.Func<TComponent, TComponent> UpdateComponent)
        where TComponent : IModel.IComponent {
        ReadableComponentStorageExtensions.UpdateComponent(this, UpdateComponent);
      }

      /// <summary>
      /// Add or replace a component
      /// </summary>
      protected virtual void AddOrUpdateComponent(IModel.IComponent toSet) {
        if(toSet is IModel.IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
          throw new System.ArgumentException($"Component of type {toSet.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
        }
        ReadableComponentStorageExtensions.AddOrUpdateComponent(this, toSet);
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(IModel.IComponent toRemove)
        => ReadableComponentStorageExtensions.RemoveComponent(this, toRemove.Key);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>()
        where TComponent : IModel.IComponent<TComponent>
          => ReadableComponentStorageExtensions.RemoveComponent<TComponent>(this);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent<TComponent>(out IComponent removed)
        where TComponent : IModel.IComponent<TComponent> {
        if(ReadableComponentStorageExtensions.RemoveComponent<TComponent>(this, out Data.IComponent found)) {
          removed = found as IModel.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove)
        => ReadableComponentStorageExtensions.RemoveComponent(this, toRemove);

      /// <summary>
      /// Remove an existing component
      /// </summary>
      protected virtual bool RemoveComponent(System.Type toRemove, out IComponent removed) {
        if(ReadableComponentStorageExtensions.RemoveComponent(this, toRemove, out Data.IComponent found)) {
          removed = found as IModel.IComponent;
          return true;
        }

        removed = null;
        return false;
      }

      /// <summary>
      /// Remove and get an existing component
      /// </summary>
      protected virtual bool RemoveComponent(string componentKeyToRemove, out IModel.IComponent removedComponent) {
        if(ReadableComponentStorageExtensions.RemoveComponent(this, componentKeyToRemove, out Data.IComponent component)) {
          removedComponent = component as IModel.IComponent;
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