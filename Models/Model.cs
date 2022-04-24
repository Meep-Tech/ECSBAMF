using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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
    /// Deserialize a model from json as a Model
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static Model FromJson(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universeOverride = null,
       params (string key, object value)[] withConfigurationParameters
     ) => (Model)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// Deserialize a model from json as a Model
    /// </summary>
    /// <typeparam name="TModel">The type to cast the produced model to. Not the same as deserializeToTypeOverride</typeparam>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static TModel FromJsonAs<TModel>(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      params (string key, object value)[] withConfigurationParameters
     ) where TModel : Model
      => (TModel)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// The universe this model was made inside of
    /// </summary>
    [JsonIgnore]
    public Universe Universe {
      get;
      internal set;
    } Universe IModel.Universe { 
      get => Universe;
      set => Universe = value; 
    }

    ///<summary><inheritdoc/></summary>
    public override bool Equals(object obj) {
      // must be this type or a child
      if(obj is not null && !GetType().IsAssignableFrom(obj.GetType())) {
        return false;
      }

      // unique are easy
      if(obj is IUnique other && this is IUnique current)
        return other.Id == current.Id;
      else {
        CompareLogic compareLogic = Universe.Models.GetCompareLogicFor(GetType());
        ComparisonResult result = compareLogic.Compare(this, obj as IModel);

        return result.AreEqual;
      }
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

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModelBase FromJson(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      params (string key, object value)[] withConfigurationParameters
     ) => (TModelBase)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <typeparam name="TModel">The type to cast the produced model to. Not the same as deserializeToTypeOverride</typeparam>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModel FromJsonAs<TModel>(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      params (string key, object value)[] withConfigurationParameters
     ) where TModel : TModelBase
      => (TModel)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// For the base configure calls
    /// </summary>
    IModel IModel.Initialize(IBuilder builder) {
      Factory = (IModel.IBuilderFactory)builder.Archetype;
      Universe
        = builder.Archetype.Id.Universe;
      return Initialize((IBuilder<TModelBase>)builder);
    }

    /// <summary>
    /// Can be used to initialize a model after the ctor call in xbam
    /// </summary>
    protected virtual Model<TModelBase> Initialize(IBuilder<TModelBase> builder)
      => this;
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
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModelBase FromJson(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universeOverride = null,
       params (string key, object value)[] withConfigurationParameters
     ) => (TModelBase)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <typeparam name="TModel">The type to cast the produced model to. Not the same as deserializeToTypeOverride</typeparam>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModel FromJsonAs<TModel>(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universeOverride = null,
       params (string key, object value)[] withConfigurationParameters
     ) where TModel : TModelBase
      => (TModel)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);

    /// <summary>
    /// Default collection of archetypes for this model type based on the Default Univese
    /// </summary>
    public static Archetype<TModelBase, TArchetypeBase>.Collection Types
      => (Archetype<TModelBase, TArchetypeBase>.Collection)
        Archetypes.DefaultUniverse.Archetypes.GetCollectionFor(typeof(TArchetypeBase));

    /// <summary>
    /// The model's archetype:
    /// </summary>
    [ArchetypeProperty]
    public TArchetypeBase Archetype {
      get;
      private set;
    } TArchetypeBase IModel<TModelBase, TArchetypeBase>.Archetype { 
      get => Archetype; 
      set => Archetype = value; 
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