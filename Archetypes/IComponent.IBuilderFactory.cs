﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial interface IComponent {

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model[] class and IComponent[] class by default.
    /// This is the base interface.
    /// </summary>
    public new interface IBuilderFactory
      : IModel.IBuilderFactory {

      /// <summary>
      /// The key for the component type.
      /// This is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
      /// </summary>
      string Key {
        get;
      }

      /// <summary>
      /// If the component made from this factory should be included in equality checks for the parent object
      /// </summary>
      public virtual bool IncludeInParentModelEqualityChecks
        => true;

      /// <summary>
      /// Make a default component for an archetype.
      /// </summary>
      /// <returns></returns>
      public IComponent Make()
        => (IComponent)(this as Archetype).MakeDefault();

      /// <summary>
      /// Make a default component for an archetype.
      /// </summary>
      /// <returns></returns>
      public IComponent Make(IBuilder parentBuilder)
        => (this as Archetype).Make<IComponent>(parentBuilder);
    }
  }

  /// <summary>
  /// extension methods for IComponent.IBuilderFactory
  /// </summary>
  /*public static class IComponentIBuilderFactoryExtensions {

    /// <summary>
    /// Make a default component for an archetype.
    /// </summary>
    /// <returns></returns>
    public static IComponent Make(this IComponent.IBuilderFactory @this)
      => (IComponent)(@this as Archetype).MakeDefault();

    /// <summary>
    /// Make a default component for an archetype.
    /// </summary>
    /// <returns></returns>
    public static IComponent Make(this IComponent.IBuilderFactory @this, IBuilder parentBuilder)
      => (@this as Archetype).Make<IComponent>(parentBuilder);
  }*/

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent
    where TComponentBase : IComponent<TComponentBase> {

    /// <summary>
    /// General Base Builder Factory for Components.
    /// </summary>
    [Configuration.Loader.Settings.DoNotBuildInInitialLoad]
    public new class BuilderFactory
      : BuilderFactory<BuilderFactory>,
      IComponent.IBuilderFactory {

      /// <summary>
      /// The key for the component type.
      /// This is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
      /// </summary>
      public string Key
        => ModelBaseType.FullName;

      /// <summary>
      /// The default way a new builder is created.
      /// This can be used to set this for a Model<> without archetypes.
      /// </summary>
      public override Func<Archetype, Dictionary<string, object>, Universe, IBuilder<TComponentBase>> BuilderConstructor {
        get => _defaultBuilderCtor ??= (archetype, @params, universe) => new IModel<TComponentBase>.Builder(archetype, @params, universe);
        set => _defaultBuilderCtor = value;
      }

      public BuilderFactory(
        Identity id,
        Universe universe = null
      )  : base(id, universe) {}

      public BuilderFactory(
        Identity id,
        Universe universe,
        HashSet<IComponent> archetypeComponents,
        IEnumerable<Func<IBuilder, IModel.IComponent>> modelComponentCtors
      )  : base(id, universe, archetypeComponents, modelComponentCtors) {}
    }
  }
}
