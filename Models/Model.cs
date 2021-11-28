using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system.
  /// This is the non-generic base class for Utility
  /// </summary>
  public partial class Model : IModel {}

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system, and uses a built in default Builder as it's base archetype.
  /// </summary>
  public partial class Model<TModelBase>
    : Model, IModel<TModelBase>
    where TModelBase : IModel<TModelBase> 
  {

    /// <summary>
    /// Invoke the static initalzer
    /// TODO: move this to the archetype loader to be called on all IModel<> types.
    /// </summary>
    static Model() {
      Models<TModelBase>.StaticInitalization?.Invoke();
    }

    /// <summary>
    /// This is the default ctor.
    /// </summary>
    protected Model(Builder @params = null) {}
  }

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system.
  /// </summary>
  public partial class Model<TModelBase, TArchetypeBase>
    : Model, IModel<TModelBase, TArchetypeBase>
    where TModelBase : IModel<TModelBase, TArchetypeBase> 
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {

    /// <summary>
    /// The model's archetype:
    /// </summary>
    public TArchetypeBase type {
      get;
    }

    protected Model(Model<TModelBase>.Builder builder = null) {
      type = builder.Type as TArchetypeBase;
    }
  }
}