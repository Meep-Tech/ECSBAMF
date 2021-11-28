using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Components {

    /// <summary>
    /// Cached model base types
    /// </summary>
    static Dictionary<string, System.Type> _modelBaseTypes
      = new Dictionary<string, Type>();

    /// <summary>
    /// Get the builder for a given component by type.d
    /// </summary>
    public static IComponent.IBuilderFactory GetBuilderFactoryFor(Type type)
      => (IComponent.IBuilderFactory)Model._factoriesByModelBase[type];

    /// <summary>
    /// Get the base model type of this model type.
    /// </summary>
    public static System.Type GetComponentBaseType(this System.Type type)
      => _modelBaseTypes.TryGetValue(type.FullName, out System.Type foundType)
        ? foundType
        : _modelBaseTypes[type.FullName] = _findComponentBaseType(type);

    /// <summary>
    /// Calculate this model's base model type.
    /// </summary>
    static System.Type _findComponentBaseType(System.Type type) {
      var potentialModelBaseType = type;
      // while we have a type to check
      while(potentialModelBaseType != null) {
        // check if this is the final base type, it must be a child of IModel then.
        if(potentialModelBaseType.BaseType == null) {
          if(typeof(IComponent).IsAssignableFrom(potentialModelBaseType)) {
            return potentialModelBaseType;
          }
        }

        potentialModelBaseType = potentialModelBaseType.BaseType;
      }

      throw new NotImplementedException($"System.Type: {type.FullName}, does not have a base Type that inherits from IModel or Model<>.");
    }
  }

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Components<TComponent> 
    where TComponent : Data.IComponent<TComponent> 
  {

    /// <summary>
    /// Can be used to set up stuff for this component before a component of this type if ever made
    /// </summary>
    public static Action StaticInitalization
      = null;

    /// <summary>
    /// The key for this type of component.
    /// This is based on the base model type's name.
    /// There should only be one component per key on a model.
    /// </summary>
    public static string Key
      => BuilderFactory.Key;

    /// <summary>
    /// Builder instance for this type of component.
    /// You can use this to set a custom builder for this type of component and it's children.
    /// </summary>
    public static IComponent<TComponent>.BuilderFactory BuilderFactory {
      get => _builderFactory ??= _findFirstInheritedFactory(typeof(TComponent));
      set {
        Model._factoriesByModelBase[typeof(TComponent)] = value;
        _builderFactory = value;
      }
    } static IComponent<TComponent>.BuilderFactory _builderFactory;

    /// <summary>
    /// Get the first factory inherited by a given model:
    /// </summary>
    static IComponent<TComponent>.BuilderFactory _findFirstInheritedFactory(Type modelType) {
      IComponent<TComponent>.BuilderFactory factory = null;
      // check if we already have one set by someone:
      if(Model._factoriesByModelBase.TryGetValue(modelType, out Model.IBuilderFactory builder)) {
        factory = (IComponent<TComponent>.BuilderFactory)builder;
      }// just the interface:
      else if(modelType.BaseType == null) {
        if(modelType.IsAssignableToGeneric(typeof(IModel<>))) {
          factory = _makeDefaultFactoryFor(modelType);
        }
      }// if we need to find the base type:
      else {
        Type baseType = modelType.BaseType;
        Type originalType = modelType;
        while(baseType != null) {
          if(baseType.IsAssignableToGeneric(typeof(IModel<>))) {
            if(Model._factoriesByModelBase.TryGetValue(baseType, out Model.IBuilderFactory factoryBuilder)) {
              factory = (IComponent<TComponent>.BuilderFactory)factoryBuilder;
              break;
            }
          }
          else {
            factory = _makeDefaultFactoryFor(originalType);
            break;
          }

          originalType = baseType;
          baseType = baseType.BaseType;
        }
      }

      Model._factoriesByModelBase[modelType] = factory
        ?? throw new NotImplementedException($"No BuilderFactory was found or built for the model type: {modelType.FullName}");
      return factory;
    }

    /// <summary>
    /// Get the default factory for a model type:
    /// </summary>
    static IComponent<TComponent>.BuilderFactory _makeDefaultFactoryFor(Type modelType)
      => new IComponent<TComponent>.BuilderFactory(
        new IComponent<TComponent>.BuilderFactory.Identity(modelType.FullName)
      );
  }
}