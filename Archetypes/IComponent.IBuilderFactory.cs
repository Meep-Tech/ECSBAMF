using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  public partial interface IComponent {

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
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
    }
  }

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent
    where TComponentBase : IComponent<TComponentBase> {

    public new class BuilderFactory
      : IModel<TComponentBase>.BuilderFactory,
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
      public override Func<Archetype, Dictionary<string, object>, IBuilder<TComponentBase>> BuilderConstructor {
        get => _defaultBuilderCtor ??= (archetype, @params) => new IModel<TComponentBase>.Builder(archetype, @params);
        set => _defaultBuilderCtor = value;
      }

      /*static BuilderFactory() {
        /// By default, these use upclassed builders:
        Components<TComponentBase>.BuilderFactory.BuilderConstructor
          = type => new Builder(type);
      }*/

      public BuilderFactory(Data.Archetype.Identity id)
        : base(id) {
      }
    }
  }
}
