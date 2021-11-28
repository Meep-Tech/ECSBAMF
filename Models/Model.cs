using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system.
  /// This is the non-generic base class for Utility
  /// </summary>
  public partial class Model 
    : IModel {}

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
    protected Model(Builder @params = null) : base() {}
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
    /// Pre-fetchable model types constant, with all potential types for a given type of model.
    /// </summary>
    public static Archetype.Collection<TModelBase, TArchetypeBase> Types
      => (Archetype.Collection<TModelBase, TArchetypeBase>)
        Archetypes.GetCollectionFor(typeof(TArchetypeBase).AsArchetype());

    /// <summary>
    /// Make shortcut.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel>(TArchetypeBase type, Action<Builder> builderConfiguration = null)
      where TDesiredModel : TModelBase
        => type.Make<TDesiredModel>(builder => { 
          builderConfiguration(builder); 
          return builder;
        });

    /// <summary>
    /// Make shortcut.
    /// </summary>
    public static TModelBase Make(TArchetypeBase type, Action<Builder> builderConfiguration = null)
        => type.Make<TModelBase>(builder => { 
          builderConfiguration(builder); 
          return builder;
        });

    /// <summary>
    /// The model's archetype:
    /// </summary>
    public TArchetypeBase type {
      get;
    }

    protected Model(Model<TModelBase>.Builder builder = null) : base() {
      type = builder.Type as TArchetypeBase;
    }
  }
}