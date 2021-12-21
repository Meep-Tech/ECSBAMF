using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Universe {
    public class ModelsData {

      /// <summary>
      /// Cached model base types
      /// </summary>
      internal Dictionary<string, System.Type> _baseTypes
        = new Dictionary<string, Type>();

      /// <summary>
      /// Cached model base types
      /// </summary>
      internal Dictionary<Archetype, System.Type> _modelTypesProducedByArchetypes
        = new Dictionary<Archetype, Type>();

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Archetype.Collection _factories;

      /// <summary>
      /// Link to the parent universe
      /// </summary>
      Universe _universe;

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Dictionary<Type, Archetype.Collection> _factoriesByModelBases
        = new Dictionary<Type, Archetype.Collection>();

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Dictionary<Type, IModel.IBuilderFactory> _factoriesByModelType
        = new Dictionary<Type, IModel.IBuilderFactory>();

      /// <summary>
      /// The compare logic organized by inheritance/types
      /// </summary>
      internal readonly Dictionary<Type, CompareLogic> _compareLogicByModelType 
        = new Dictionary<Type, CompareLogic>();

      internal ModelsData(Universe universe) {
        _factories
          = new Archetype.Collection(universe);
        universe.Archetypes._collectionsByRootArchetype
          .Add(typeof(IModel.IBuilderFactory).FullName, _factories);
        this._universe = universe;
      }

      /// <summary>
      /// Get the builder factory for a given type
      /// </summary>
      public IModel.IBuilderFactory GetBuilderFactoryFor(System.Type systemType)
        => typeof(Model<,>).IsAssignableFrom(systemType)
          ? throw new NotImplementedException()
          : _factoriesByModelType.TryGetValue(systemType, out var foundFactory)
            ? foundFactory
            : _findFirstInheritedFactory(systemType);

      /// <summary>
      /// Get the builder factory for a given type
      /// </summary>
      public CompareLogic GetCompareLogicFor(System.Type systemType)
        => _compareLogicByModelType.TryGetValue(systemType, out var found)
            ? found
            : _findFirstInheritedCompareLogic(systemType);

      /// <summary>
      /// Get the builder factory for a given type
      /// </summary>
      public IModel.IBuilderFactory GetBuilderFactoryFor<TModel>()
        where TModel : IModel<TModel>
          => GetBuilderFactoryFor(typeof(TModel));

      /// <summary>
      /// Set the builder factory for a type of component.
      /// TODO: Must be doen during init
      /// </summary>
      public void SetBuilderFactoryFor<TModel>(IModel.IBuilderFactory factory)
        where TModel : IModel<TModel>
          => _universe.Models._factoriesByModelType[typeof(TModel)]
            = factory;

      /// <summary>
      /// Update the EFCore entity builder for this model type
      /// </summary>
      public void ModifyEfCoreBuilderFor<TModel>(Action<EntityTypeBuilder> action)
        where TModel : IModel<TModel> {
        if(_universe.ModelSerializer.Options.TypesToMapToDbContext.TryGetValue(typeof(TModel), out var found)) {
          _universe.ModelSerializer.Options.TypesToMapToDbContext[typeof(TModel)] = action + found;
        }
        else {
          _universe.ModelSerializer.Options.TypesToMapToDbContext[typeof(TModel)] = action;
        }
      }

      /// <summary>
      /// Get the model type an archetype should produce by default.
      /// </summary>
      public Type GetModelTypeProducedBy(Archetype archetype)
        => _modelTypesProducedByArchetypes[archetype];

      /// <summary>
      /// Get the base model type of this model type.
      /// </summary>
      public System.Type GetModelBaseType(System.Type type)
        => _baseTypes.TryGetValue(type.FullName, out System.Type foundType)
          ? foundType
          : _baseTypes[type.FullName] = _findModelBaseType(type);

      /// <summary>
      /// Calculate this model's base model type.
      /// </summary>
      System.Type _findModelBaseType(System.Type type) {
        var potentialModelBaseType = type;
        Type potentialAbstractModelBaseType = null;
        // while we have a type to check
        while(potentialModelBaseType != null) {
          // if this model type is the first abstract one we encountered, it may be the model base
          if(potentialModelBaseType.BaseType.IsAbstract && potentialAbstractModelBaseType is null) {
            potentialAbstractModelBaseType = potentialModelBaseType;
          }
          ///potentialAbstractModelBaseType can't be the model base if there's a non abstract type between it and the potential model base
          if(!(potentialAbstractModelBaseType is null) && !potentialAbstractModelBaseType.BaseType.IsAbstract) {
            potentialAbstractModelBaseType = null;
          }

          // if we have an abstract match and are hitting generics, see if we're in the base classes
          if(potentialAbstractModelBaseType != null
            && potentialModelBaseType.IsGenericType) {
            if(potentialModelBaseType.GetGenericTypeDefinition().Equals(typeof(Model<>))) {
              return potentialAbstractModelBaseType;
            }
            if(potentialModelBaseType.GetGenericTypeDefinition().Equals(typeof(Model<,>))) {
              return potentialAbstractModelBaseType;
            }
            if(potentialModelBaseType.GetGenericTypeDefinition().Equals(typeof(Model<>.WithComponents))) {
              return potentialAbstractModelBaseType;
            }
            if(potentialModelBaseType.GetGenericTypeDefinition().Equals(typeof(Model<,>.WithComponents))) {
              return potentialAbstractModelBaseType;
            }
          }

          // check if this is the final base type, it must be a child of IModel then.
          if(potentialModelBaseType.BaseType == null) {
            if(typeof(IModel).IsAssignableFrom(potentialModelBaseType)) {
              return potentialModelBaseType;
            }
          }

          potentialModelBaseType = potentialModelBaseType.BaseType;
        }
        if(potentialAbstractModelBaseType != null) {
          return potentialAbstractModelBaseType;
        }

        throw new NotImplementedException($"System.Type: {type.FullName}, does not have a base Type that inherits from IModel<>.");
      }

      /// <summary>
      /// Set a new constructor for this model's builder class.
      /// </summary>
      public void SetBuilderConstructor<TModel>(Func<IModel<TModel>.Builder, TModel> newConstructor)
        where TModel : IModel<TModel> {
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
            if(typeof(IModel).IsAssignableFrom(baseType)) {
              if(baseType.BaseType?.FullName == typeof(Model).FullName) {
                factory = _makeDefaultFactoryFor(originalType);
                break;
              }
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
      /// Get the first factory inherited by a given model:
      /// </summary>
      CompareLogic _findFirstInheritedCompareLogic(Type modelType) {
        if(!modelType.IsAssignableToGeneric(typeof(IModel<>))) {
          throw new NotImplementedException(
            $"Model Type: {modelType.FullName} does not inherit from Model<TModelBase>." +
            $" If you are using Model<TModelBase, TArchetypeBase> then the Archetype " +
            $"Base would be the default FactoryBuilder, and this should variable not be used."
          );
        }

        // check if we already have one set by someone:
        if(_compareLogicByModelType.TryGetValue(modelType, out CompareLogic compareLogic)) {
          ///// Do nothing
        }// just the interface:
        else if(modelType.BaseType == null) {
          if(modelType.IsAssignableToGeneric(typeof(IModel<>))) {
            compareLogic = new CompareLogic();
          }
        }// if we need to find the base type:
        else {
          Type baseType = modelType.BaseType;
          while(baseType != null) {
            if(typeof(IModel).IsAssignableFrom(baseType)) {
              if(baseType.BaseType?.FullName == typeof(Model).FullName) {
                compareLogic = new CompareLogic();
                break;
              }
              if(_compareLogicByModelType.TryGetValue(baseType, out compareLogic)) {
                break;
              }
            }
            else {
              compareLogic = new CompareLogic();
              break;
            }

            baseType = baseType.BaseType;
          }
        }

        _compareLogicByModelType[modelType] = compareLogic
          ?? throw new NotImplementedException($"No CompareLogic was found or built for the model type: {modelType.FullName}");
        return compareLogic;
      }

      /// <summary>
      /// Make the default factory for a model type using reflection:
      /// </summary>
      IModel.IBuilderFactory _makeDefaultFactoryFor(Type modelType) {
        Type builderType;
        Type builderIdType;
        System.Reflection.ConstructorInfo ctor;

        // component
        if(modelType.IsAssignableToGeneric(typeof(IComponent<>))) {
          builderType = typeof(IComponent<>.BuilderFactory).MakeGenericType(modelType);
          builderIdType = typeof(IComponent<>.BuilderFactory.Identity)
            .MakeGenericType(modelType, builderType);

          ctor = builderType
            .GetConstructor(
              System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public,
              null,
              new Type[] { builderIdType, typeof(Universe) },
              null
            );

          return ctor.Invoke(new object[] {
            Activator.CreateInstance(
              builderIdType,
              "Default",
              "Component.Factory"
            ),
            _universe
          }) as IComponent.IBuilderFactory;
        }

        // model
        builderType = typeof(IModel<>.BuilderFactory).MakeGenericType(modelType);
        builderIdType = typeof(IModel<>.BuilderFactory.Identity)
          .MakeGenericType(modelType, builderType);

        ctor = builderType
            .GetConstructor(
              System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public,
              null,
              new Type[] { 
                builderIdType,
                typeof(Universe)
              },
              null
            );

        return ctor.Invoke(new object[] {
            Activator.CreateInstance(
              builderIdType,
              "Default",
              "Model.Factory"
            ),
            _universe
          }) as IModel.IBuilderFactory;
      }
    }
  }
}
