using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data {

  public partial interface IComponent {

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// This is the base interface.
    /// </summary>
    public interface IBuilderFactory
      : Model.IBuilderFactory {

      /// <summary>
      /// The key for the component type
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

    public class BuilderFactory
      : Model<TComponentBase>.BuilderFactory,
      IComponent.IBuilderFactory {

      /// <summary>
      /// The key for the component type
      /// </summary>
      public string Key
        => ModelBaseType.FullName;

      static BuilderFactory() {
        /// By default, these use struct based builders
        Components<TComponentBase>.BuilderFactory.NewBuilderConstructor
          = type => new Builder(type);
      }

      public BuilderFactory(Data.Archetype.Identity id)
        : base(id) {
      }
    }
  }
}
