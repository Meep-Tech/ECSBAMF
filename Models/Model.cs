using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system.
  /// This is the non-generic base class for Utility
  /// </summary>
  public abstract partial class Model
    : IModel 
  {

    /// <summary>
    /// The universe this model was made inside of
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public Universe Universe {
      get;
      private set;
    }

    /// <summary>
    /// For the base configure calls
    /// </summary>
    IModel IModel.Configure(IBuilder @params)
      => _initialize(@params);

    /// <summary>
    /// Initialization logic
    /// </summary>
    internal virtual Model _initialize(IBuilder builder) {
      Universe
        = builder.Archetype.Id.Universe;

      return this;
    }
  }

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system, and uses a built in default Builder as it's base archetype.
  /// </summary>
  public abstract partial class Model<TModelBase>
    : Model, IModel<TModelBase>
    where TModelBase : Model<TModelBase>
  {

    internal override Model _initialize(IBuilder builder) {
      Model model = base._initialize(builder);
      Factory = (IModel.IBuilderFactory)builder.Archetype;
      model = (model as Model<TModelBase>)
        .Initialize((IBuilder<TModelBase>)builder);

      return model;
    }

    /// <summary>
    /// Can be used to initialize a model after the ctor call in xbam
    /// </summary>
    protected virtual TModelBase Initialize(IBuilder<TModelBase> builder)
      => (TModelBase)this;
  }

  /// <summary>
  /// The base class for a mutable data model that can be produced by an Archetype.
  /// This includes a components system.
  /// </summary>
  public abstract partial class Model<TModelBase, TArchetypeBase>
    : Model, IModel<TModelBase, TArchetypeBase>
    where TModelBase : IModel<TModelBase, TArchetypeBase> 
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {

    /// <summary>
    /// Default collection of archetypes for this model type based on the Default Univese
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.ArchetypeCollection Types
      => (Archetype<TModelBase, TArchetypeBase>.ArchetypeCollection)
        Archetypes.DefaultUniverse.Archetypes.GetCollectionFor(typeof(TArchetypeBase));

    /// <summary>
    /// The model's archetype:
    /// </summary>
    [IsArchetypeProperty]
    [JsonConverter(typeof(Archetype.ToKeyStringConverter))]
    public TArchetypeBase Archetype {
      get;
      private set;
    }

    internal override Model _initialize(IBuilder builder) {
      Model model = base._initialize(builder);
      Archetype = builder?.Archetype as TArchetypeBase;

      return model;
    } 

    /// <summary>
    /// Make shortcut.
    /// </summary>
    public static TDesiredModel Make<TDesiredModel>(TArchetypeBase type, Action<IModel.Builder> builderConfiguration = null)
      where TDesiredModel : TModelBase
        => type.Make<TDesiredModel>(builder => { 
          builderConfiguration(builder); 
          return builder;
        });

    /// <summary>
    /// Make shortcut.
    /// </summary>
    public static TModelBase Make(TArchetypeBase type, Action<IModel.Builder> builderConfiguration = null)
        => type.Make<TModelBase>(builder => { 
          builderConfiguration(builder); 
          return builder;
        });
  }
}