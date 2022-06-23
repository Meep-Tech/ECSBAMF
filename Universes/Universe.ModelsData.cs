﻿using KellermanSoftware.CompareNetObjects;
using Meep.Tech.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Universe {

    /// <summary>
    /// Data for the models in a Xbam universe.
    /// </summary>
    public class ModelsData {

      /// <summary>
      /// The number of different loaded model types.
      /// </summary>
      public int Count
        => _baseTypes.Count;

      /// <summary>
      /// Dependencies for different types.
      /// </summary>
      public IReadOnlyDictionary<System.Type, IEnumerable<System.Type>> Dependencies
        => _dependencies; internal Dictionary<System.Type, IEnumerable<System.Type>> _dependencies
          = new();

      /// <summary>
      /// Stores simple access to all model types that can be produced.
      /// </summary>
      public IEnumerable<System.Type> All
        => _baseTypes.Values;

      /// <summary>
      /// Cached model base types
      /// </summary>
      internal Dictionary<string, System.Type> _baseTypes
        = new();

      /// <summary>
      /// Cached model base types
      /// </summary>
      internal Dictionary<Archetype, System.Type> _modelTypesProducedByArchetypes
        = new();

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Archetype.Collection _factories;

      /// <summary>
      /// Link to the parent universe
      /// </summary>
      public Universe Universe
        => _universe;
      Universe _universe;

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Dictionary<Type, Archetype.Collection> _factoriesByModelBases
        = new();

      /// <summary>
      /// The collection of all base model BuilderFactories.
      /// </summary>
      internal Dictionary<Type, IModel.IBuilderFactory> _factoriesByModelType
        = new();

      /// <summary>
      /// The compare logic organized by inheritance/types
      /// </summary>
      internal readonly Dictionary<Type, CompareLogic> _compareLogicByModelType 
        = new();

      internal ModelsData(Universe universe) {
        _factories
          = new Archetype.Collection(universe);
        universe.Archetypes._collectionsByRootArchetype
          .Add(typeof(IModel.IBuilderFactory).FullName, _factories);
        _universe = universe;
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
      IModel.IBuilderFactory _findFirstInheritedFactory(Type modelType)
        => _findFirstInheritedDataItemFor(
          modelType,
          type => _factoriesByModelType.TryGetValue(type, out var factory)
            ? factory
            : null,
          type => _makeDefaultFactoryFor(modelType),
          (type, newValue) => _factoriesByModelType[type] = newValue
        );

      /*IModel.IBuilderFactory _findFirstInheritedFactory(Type modelType) {
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
          Type currentValidType = originalType;
          while(baseType != null) {
              if (typeof(IModel).IsAssignableFrom(baseType)) {
                if (baseType.BaseType?.FullName == typeof(Model).FullName) {
                  factory = _makeDefaultFactoryFor(originalType);
                  break;
                }
                if (_factoriesByModelType.TryGetValue(baseType, out factory)) {
                  break;
                }
                if (baseType.Name == typeof(Model<>.WithComponents).Name
                    && baseType.Module == typeof(Model<>.WithComponents).Module) {
                  factory = _makeDefaultFactoryFor(originalType);
                  break;
                }
              } else {
                factory = _makeDefaultFactoryFor(originalType);
                break;
              }

            if (!baseType.IsAbstract) {
              currentValidType = baseType;
            }

            originalType = baseType;
            baseType = baseType.BaseType;
          }

          if (factory is null) {
            factory = _makeDefaultFactoryFor(currentValidType);
          }
        }


        _factoriesByModelType[modelType] = factory
          ?? throw new NotImplementedException($"No BuilderFactory was found or built for the model type: {modelType.FullName}");
        return factory;
      }*/

      /// <summary>
      /// Get the first factory inherited by a given model:
      /// </summary>
      TDataItem _findFirstInheritedDataItemFor<TDataItem>(Type modelType, Func<Type, TDataItem> cachedItemFetcher, Func<Type, TDataItem> defaultBulder, Action<Type, TDataItem> newItemCacheSetter) {
        if(!modelType.IsAssignableToGeneric(typeof(IModel<>))) {
          throw new NotImplementedException(
            $"Model Type: {modelType.FullName} does not inherit from Model<TModelBase>." +
            $" If you are using Model<TModelBase, TArchetypeBase> then the Archetype " +
            $"Base would be the default FactoryBuilder, and this method of property searching/setting should not be used."
          );
        }

        TDataItem found = cachedItemFetcher(modelType);
        // check if we already have one set by someone:
        if(found is not null) {
          ///// Do nothing
        }// just the interface:
        else if(modelType.BaseType == null) {
          if(modelType.IsAssignableToGeneric(typeof(IModel<>))) {
            found = defaultBulder(modelType);
          }
        }// if we need to find the base type:
        else {
          Type baseType = modelType.BaseType;
          Type originalType = modelType;
          Type currentValidType = originalType;
          while(baseType != null) {
            if (typeof(IModel).IsAssignableFrom(baseType)) {
              if (baseType.BaseType?.FullName == typeof(Model).FullName) {
                found = defaultBulder(originalType);
                break;
              }
              if ((found = cachedItemFetcher(baseType)) != null) {
                break;
              }
              if (baseType.Name == typeof(Model<>.WithComponents).Name
                  && baseType.Module == typeof(Model<>.WithComponents).Module) {
                found = defaultBulder(originalType);
                break;
              }
            } else {
              found = defaultBulder(originalType);
              break;
            }

            if (!baseType.IsAbstract) {
              currentValidType = baseType;
            }

            originalType = baseType;
            baseType = baseType.BaseType;
          }

          if (found is null) {
            found = defaultBulder(currentValidType);
          }
        }

        newItemCacheSetter(modelType, found
          ?? throw new NotImplementedException($"No {typeof(TDataItem).Name} was found , or could be built for the model type: {modelType.FullName}"));
        return found;
      }
      
      /// <summary>
      /// Get the first factory inherited by a given model:
      /// TODO: this and the find first factory logic should be combined into one, with find first factory taking precidence. It's more up to date.
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
            compareLogic = _makeDefaultCompareLogic();
          }
        }// if we need to find the base type:
        else {
          Type baseType = modelType.BaseType;
          while(baseType != null) {
            if(typeof(IModel).IsAssignableFrom(baseType)) {
              if(baseType.BaseType?.FullName == typeof(Model).FullName) {
                compareLogic = _makeDefaultCompareLogic();
                break;
              }
              if(_compareLogicByModelType.TryGetValue(baseType, out compareLogic)) {
                break;
              }
            }
            else {
              compareLogic = _makeDefaultCompareLogic();
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
      /// Makes the default compare logic using this universes settings
      /// </summary>
      CompareLogic _makeDefaultCompareLogic()
        => new(_universe.ModelSerializer.Options.DefaultComparisonConfig);

      /// <summary>
      /// Make the default factory for a model type using reflection:
      /// </summary>
      IModel.IBuilderFactory _makeDefaultFactoryFor(Type modelType) {
        Type builderType = null;
        System.Reflection.ConstructorInfo ctor;

        Type builderIdType;
        // component
        if (modelType.IsAssignableToGeneric(typeof(IComponent<>))) {
          try {
            builderType = typeof(IComponent<>.BuilderFactory).MakeGenericType(modelType);
            builderIdType = typeof(IComponent<>.BuilderFactory.Identity)
              .MakeGenericType(modelType, builderType);

            if (builderIdType.ContainsGenericParameters || builderIdType.ContainsGenericParameters) {
              throw new Meep.Tech.Data.Configuration.Loader.CannotInitializeArchetypeException(
                $"Cannot create a default XBam Factory for a component type that requires generic parameters:\n Builder Type: {builderType.ToFullHumanReadableNameString()},\nBuilder Id Type:{builderIdType.ToFullHumanReadableNameString()}"
              );
            }
          }
          catch (Exception e) {
            throw new ArgumentException($"Could not apply generics to Builder or Id for XBam Component of Type {modelType.FullName}. Using Component Type: {modelType?.ToString() ?? "null"} and Builder Tyoe:{builderType?.ToString() ?? "null"}  \n Inner Exception: {e} \n ===============");
          }

          ctor = builderType
              .GetConstructor(
                System.Reflection.BindingFlags.NonPublic
                  | System.Reflection.BindingFlags.Instance
                  | System.Reflection.BindingFlags.Public,
                null,
                new Type[] { null, typeof(Universe) },
                null
              );

          return ctor.Invoke(new object[] {
            Activator.CreateInstance(
null,
              "Default",
              "Component.Factory",
              _universe,
              null
            ),
            _universe
          }) as IComponent.IBuilderFactory;
        }

        // model
        try {
          builderType = typeof(IModel<>.BuilderFactory).MakeGenericType(modelType);
          builderIdType = typeof(IModel<>.BuilderFactory.Identity)
            .MakeGenericType(modelType, builderType);
          if (builderIdType.ContainsGenericParameters || builderIdType.ContainsGenericParameters) {
            throw new Meep.Tech.Data.Configuration.Loader.CannotInitializeArchetypeException(
              $"Cannot create a default XBam Factory for a model type that requires generic parameters:\n Builder Type: {builderType.ToFullHumanReadableNameString()},\nBuilder Id Type:{builderIdType.ToFullHumanReadableNameString()}"
            );
          }
        } catch (Exception e) {
          throw new ArgumentException($"Could not apply generics to Builder or Id for XBam Model of Type {modelType.FullName}. Using Model Type: {modelType?.ToString() ?? "null"} and Builder Tyoe:{builderType?.ToString() ?? "null"}  \n Inner Exception: {e} \n ===============");
        }

        ctor = builderType
            .GetConstructor(
              System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public,
              null,
              new Type[] {
                null,
                typeof(Universe)
              },
              null
            );

        return ctor.Invoke(new object[] {
            Activator.CreateInstance(
              null,
              "Default",
              "Model.Factory",
              _universe,
              null
            ),
            _universe
          }) as IModel.IBuilderFactory;
      }


      /// <summary>
      /// Make an object ctor from a provided default ctor.
      /// Valid CTORS:
      ///  - public|private|protected Model(IBuilder builder)
      ///  - public|private|protected Model()
      /// </summary>
      internal Func<IBuilder<TModel>, TModel> _getDefaultCtorFor<TModel>(Type modelType)
        where TModel : IModel<TModel>
          => builder => (TModel)_getDefaultCtorFor(modelType)(builder);

      /// <summary>
      /// Make an object ctor from a provided default ctor.
      /// Valid CTORS:
      ///  - public|private|protected Model(IBuilder builder)
      ///  - public|private|protected Model()
      /// </summary>
      internal Func<IBuilder, IModel> _getDefaultCtorFor(Type modelType) {
        // try to get any matching builder ctor:
        System.Reflection.ConstructorInfo ctor = modelType.GetConstructors(
          System.Reflection.BindingFlags.Public
          | System.Reflection.BindingFlags.NonPublic
          | System.Reflection.BindingFlags.Instance
        // TODO: add an attribute to specify highest priority
        // sort by priority:
        ).Select(constructor => {
          var @params = constructor.GetParameters();
          if (@params.Length > 0) {
            if (@params.Length == 1) {
              if (@params[0].ParameterType.IsAssignableToGeneric(typeof(IBuilder<>))) {
                return (constructor.IsFamily || constructor.IsPublic ? 3 : 2, constructor);
              }
            }

            // non compatable
            return (0, constructor);
          } // if there's an empty ctor, return that one
          else {
            return (1, constructor);
          }
        })
        // remove incompatable ctors before the sort and pick
        .Where(rankedConstructor => rankedConstructor.Item1 > 0)
        .OrderByDescending(rankedConstructor => rankedConstructor.Item1)
        .FirstOrDefault().constructor;

        // no args ctor:
        if(!(ctor is null) && ctor.GetParameters().Length == 0) {
          //TODO: is there a faster way to cache this?
          return (builder) => {
            return (IModel)ctor.Invoke(null);
          };
        }

        // structs may use the activator
        if(ctor is null && modelType.IsValueType) {
          Func<IBuilder, IModel> activator = _
            => (IModel)Activator.CreateInstance(modelType);
          try {
            if(!(activator.Invoke(null) is null)) {
              return activator;
            }
          } catch(Exception e) {
            throw new NotImplementedException($"No Ctor that takes a single argument thet inherits from IBuilder<TModelBase>, or 0 arguments found for Model type: {modelType.FullName}. An activator could also not be built for the type.", e);
          }
        }

        if(ctor is null) {
          throw new NotImplementedException($"No Ctor that takes a single argument thet inherits from IBuilder<TModelBase>, or 0 arguments found for Model type: {modelType.FullName}.");
        }

        //TODO: is there a faster way to cache this?
        return builder
          => (IModel)ctor.Invoke(new object[] { builder });
      }
    }
  }
}
