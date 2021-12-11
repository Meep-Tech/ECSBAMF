using Meep.Tech.Data.Configuration;
using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// A global collection of arechetypes.
  /// This is what the loader builds.
  /// </summary>
  public partial class Universe {

    /// <summary>
    /// The loader used to build this universe
    /// </summary>
    public Loader Loader {
      get;
      internal set;
    }

    /// <summary>
    /// The model serializer instance for this universe
    /// </summary>
    public Model.Serializer ModelSerializer {
      get;
      internal set;
    }

    /// <summary>
    /// Archetypes data
    /// </summary>
    public ArchetypesData Archetypes {
      get;
    }

    /// <summary>
    /// Models data
    /// </summary>
    public ModelsData Models {
      get;
    }

    /// <summary>
    /// Components data
    /// </summary>
    public ComponentsData Components {
      get;
    }

    /// <summary>
    /// Make a new universe of Archetypes
    /// </summary>
    public Universe(Loader loader) {
      Loader = loader;
      Archetypes = new ArchetypesData(this);
      Models = new ModelsData(this);
      Components = new ComponentsData(this);

      // set this as the default universe if there isn't one yet
      Data.Archetypes.DefaultUniverse ??= this;
    }

    public class ModelsData {

      /// <summary>
      /// Cached model base types
      /// </summary>
      Dictionary<string, System.Type> _modelBaseTypes
        = new Dictionary<string, Type>();

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Archetype.Collection _factories;

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Dictionary<Type, Archetype.Collection> _factoriesByModelBases;

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Dictionary<Type, IModel.IBuilderFactory> _factoriesByModelType
        = new Dictionary<Type, IModel.IBuilderFactory>();

      internal ModelsData(Universe universe) {
        _factories
          = new Archetype.Collection(universe);
      }

      /// <summary>
      /// Get the builder factory for a given type
      /// </summary>
      public IModel.IBuilderFactory GetBuilderFactoryFor(System.Type systemType) 
        => _factoriesByModelType.TryGetValue(systemType, out var foundFactory)
          ? foundFactory
          : _findFirstInheritedFactory(systemType);

      /// <summary>
      /// Get the builder factory for a given type
      /// </summary>
      public IModel.IBuilderFactory GetBuilderFactoryFor<TModel>() 
        where TModel : IModel<TModel>
          => GetBuilderFactoryFor(typeof(TModel));

      /// <summary>
      /// Get the base model type of this model type.
      /// </summary>
      public System.Type GetModelBaseType(System.Type type)
        => _modelBaseTypes.TryGetValue(type.FullName, out System.Type foundType)
          ? foundType
          : _modelBaseTypes[type.FullName] = _findModelBaseType(type);

      /// <summary>
      /// Calculate this model's base model type.
      /// </summary>
      System.Type _findModelBaseType(System.Type type) {
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

      /// <summary>
      /// Set a new constructor for this model's builder class.
      /// </summary>
      public void SetBuilderConstructor<TModel>(Func<IModel<TModel>.Builder, TModel> newConstructor) 
        where TModel : IModel<TModel> 
      {
        _factoriesByModelType[typeof(TModel)].ModelConstructor 
          = builder => newConstructor((IModel<TModel>.Builder)builder);
      }

      /// <summary>
      /// Get the first factory inherited by a given model:
      /// </summary>
      IModel.IBuilderFactory _findFirstInheritedFactory(Type modelType) {
        if(!modelType.IsAssignableToGeneric(typeof(IModel<>))) {
          throw new NotImplementedException(
            $"Model Type: {modelType.FullName} does not inherit from Model<TModelBase>." +
            $" If you are using Model<TModelBase, TArchetypeBase> then the Archetype " +
            $"Base would be the default FactoryBuilder, and this should variable not be used."
          );
        }

        IModel.IBuilderFactory factory;
        // check if we already have one set by someone:
        if(_factoriesByModelType.TryGetValue(modelType, out factory)) {
          ///// Do nothing
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
              if(_factoriesByModelType.TryGetValue(baseType, out factory)) {
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

        _factoriesByModelType[modelType] = factory
          ?? throw new NotImplementedException($"No BuilderFactory was found or built for the model type: {modelType.FullName}");
        return factory;
      }

      /// <summary>
      /// Make the default factory for a model type using reflection:
      /// </summary>
      static IModel.IBuilderFactory _makeDefaultFactoryFor(Type modelType) {
        Type builderType;
        Type builderIdType;
        if(modelType.IsAssignableToGeneric(typeof(IComponent<>))) {
          builderType = typeof(IComponent<>.BuilderFactory).MakeGenericType(modelType);
          builderIdType = typeof(IComponent<>.BuilderFactory.Identity)
            .MakeGenericType(modelType, builderType);
          return Activator.CreateInstance(
            builderType,
            Activator.CreateInstance(
              builderIdType,
              modelType.FullName
            )
          ) as IComponent.IBuilderFactory;
        }

        builderType = typeof(IModel<>.BuilderFactory).MakeGenericType(modelType);
        builderIdType = typeof(IModel<>.BuilderFactory.Identity).MakeGenericType(modelType);
        return Activator.CreateInstance(
          builderType,
          Activator.CreateInstance(
            builderIdType,
            modelType.FullName
          )
        ) as IModel.IBuilderFactory;
      }
    }

    public class ComponentsData {

      /// <summary>
      /// Cached model base types
      /// </summary>
      Dictionary<string, System.Type> _baseTypes
        = new Dictionary<string, Type>();

      Universe _universe;

      public ComponentsData(Universe universe) {
        _universe = universe;
      }

      /// <summary>
      /// Get the builder for a given component by type.d
      /// </summary>
      public IComponent.IBuilderFactory GetBuilderFactoryFor(Type type)
        => (IComponent.IBuilderFactory)_universe.Models.GetBuilderFactoryFor(type);

      /// <summary>
      /// Get the builder for a given component by type.d
      /// </summary>
      public IComponent.IBuilderFactory GetBuilderFactoryFor<TComponent>()
        where TComponent : IComponent<TComponent>
          => (IComponent.IBuilderFactory)_universe.Models.GetBuilderFactoryFor<TComponent>();

      /// <summary>
      /// Get the base model type of this component type.
      /// </summary>
      public System.Type GetComponentBaseType(System.Type type)
        => _baseTypes.TryGetValue(type.FullName, out System.Type foundType)
          ? foundType
          : _baseTypes[type.FullName] = _findComponentBaseType(type);

      /// <summary>
      /// Calculate this model's base model type.
      /// </summary>
      System.Type _findComponentBaseType(System.Type type) {
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
  }
}
