using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// Constants and static access for different types of Models
  /// </summary>
  public static class Models {

    /// <summary>
    /// Cached model base types
    /// </summary>
    static Dictionary<string, System.Type> _modelBaseTypes
      = new Dictionary<string, Type>();

    /// <summary>
    /// Get the base model type of this model type.
    /// </summary>
    public static System.Type GetModelBaseType(this System.Type type)
      => _modelBaseTypes.TryGetValue(type.FullName, out System.Type foundType)
        ? foundType
        : _modelBaseTypes[type.FullName] = _findModelBaseType(type);

    /// <summary>
    /// Calculate this model's base model type.
    /// </summary>
    static System.Type _findModelBaseType(System.Type type) {
      var potentialModelBaseType = type;
      // while we have a type to check
      while(potentialModelBaseType != null) {
        // check if this is the final base type, it must be a child of IModel then.
        if(potentialModelBaseType.BaseType == null) {
          if(typeof(IModel).IsAssignableFrom(potentialModelBaseType)) {
            return potentialModelBaseType;
          }
        }
        else if(potentialModelBaseType.BaseType.IsGenericType
          && potentialModelBaseType.BaseType.GetGenericTypeDefinition() == typeof(Model<>)) {
          return potentialModelBaseType;
        }

        potentialModelBaseType = potentialModelBaseType.BaseType;
      }

      throw new NotImplementedException($"System.Type: {type.FullName}, does not have a base Type that inherits from IModel or Model<>.");
    }
  }

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Models<TModel> 
    where TModel : IModel<TModel> 
  {

    /// <summary>
    /// Can be used to set up stuff for this component before a component of this type if ever made
    /// TOOD: since components are Intefaces, this may need to be called manually
    /// </summary>
    public static Action StaticInitalization
      = null;

    /// <summary>
    /// Builder instance for this type of component.
    /// You can use this to set a custom builder for this type of model and it's children.
    /// </summary>
    public static Model<TModel>.BuilderFactory BuilderFactory {
      get => _builderFactory ??= _findFirstInheritedFactory(typeof(TModel));
      set {
        // TODO: make sure archetype loader is locked.
        Model._factoriesByModelBase[typeof(TModel)] = value;
        _builderFactory = value;
      }
    } static Model<TModel>.BuilderFactory _builderFactory;

    /// <summary>
    /// Set a new constructor for this model's builder class.
    /// </summary>
    public static void SetBuilderConstructor(Func<Model<TModel>.Builder, TModel> newConstructor) {
      BuilderFactory.ModelConstructor = newConstructor;
    }

    /// <summary>
    /// Get the first factory inherited by a given model:
    /// </summary>
    static Model<TModel>.BuilderFactory _findFirstInheritedFactory(Type modelType) {
      if(!modelType.IsAssignableToGeneric(typeof(Model<TModel>))) {
        throw new NotImplementedException($"Model Type: {modelType.FullName} does not inherit from Model<TModelBase>. If you are using Model<TModelBase, TArchetypeBase> then the Archetype Base would be the default FactoryBuilder, and this should variable not be used.")
      }

      Model<TModel>.BuilderFactory factory = null;
      // check if we already have one set by someone:
      if(Model._factoriesByModelBase.TryGetValue(modelType, out Model.IBuilderFactory builder)) {
        factory = (Model<TModel>.BuilderFactory)builder;
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
              factory = (Model<TModel>.BuilderFactory)factoryBuilder;
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
    static Model<TModel>.BuilderFactory _makeDefaultFactoryFor(Type modelType) 
      => new Model<TModel>.BuilderFactory(
        new Model<TModel>.BuilderFactory.Identity(modelType.FullName)
      );
  }
}