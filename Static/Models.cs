﻿using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// Constants and static access for different types of Models
  /// </summary>
  public static class Models {
    
    /// <summary>
    /// The default universe to use for models
    /// TODO: this should clear some things in the future if it's changed during runtime, such as the default builder factory in the generic Models<> class.
    /// </summary>
    public static Universe DefaultUniverse {
      get => _defaultUniverseOverride ??= Archetypes.DefaultUniverse;
      set => _defaultUniverseOverride = value;
    } private static Universe _defaultUniverseOverride;

    /// <summary>
    /// Get the builder for a given component by type.d
    /// </summary>
    public static IModel.IBuilderFactory GetBuilderFactoryFor(Type type)
      => DefaultUniverse.Models._factoriesByModelType[type];

    /// <summary>
    /// Get the base model type of this model type.
    /// </summary>
    public static System.Type GetModelBaseType(this System.Type type)
      => DefaultUniverse.Models.GetModelBaseType(type);
  }

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Models<TModel> 
    where TModel : IModel<TModel> 
  {

    /// <summary>
    /// Builder instance for this type of component.
    /// You can use this to set a custom builder for this type of model and it's children.
    /// </summary>
    public static IModel<TModel>.BuilderFactory BuilderFactory {
      get => (IModel<TModel>.BuilderFactory)
        Models.DefaultUniverse.Models
          .GetBuilderFactoryFor<TModel>();
      set {
        // TODO: move this to a set function under Universe.Models for IModel<TModel> types
        // ... and add a check to make sure the universe isn't locked.
        Models.DefaultUniverse.Models
          ._factoriesByModelType[typeof(TModel)] = value;
      }
    }

    /// <summary>
    /// Set a new constructor for this model's builder class.
    /// </summary>
    public static void SetBuilderConstructor(Func<IModel<TModel>.Builder, TModel> newConstructor)
      => Models.DefaultUniverse.Models.SetBuilderConstructor(newConstructor);
  }
}