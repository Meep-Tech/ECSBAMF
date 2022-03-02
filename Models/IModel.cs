using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// This is the non generic for Utility only, extend IModel[], or IModel[,] instead.
  /// </summary>
  public partial interface IModel {

    /// <summary>
    /// Deserialize a model from a json object
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize use 
    ///    a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public static IModel FromJson(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      IBuilder withConfigurationParameters = null
    ) {
      string key;
      Universe universe = universeOverride;
      string compoundKey = jObject.Value<string>(nameof(Archetype).ToLower());
      string[] parts = compoundKey.Split('@');
      if(parts.Length == 1) {
        key = compoundKey;
        universe ??= Models.DefaultUniverse;
      }
      else if(parts.Length == 2) {
        key = parts[0];
        universe ??= Universe.Get(parts[1]);
      }
      else
        throw new ArgumentException($"No __key_ identifier provided in component data: \n{jObject}");

      string json = jObject.ToString();
      Type deserializeToType = deserializeToTypeOverride
        ?? universe.Models.GetModelTypeProducedBy(
          universe.Archetypes.All.Get(key)
        );
      object model = JsonConvert.DeserializeObject(
        json,
        deserializeToType,
        universe.ModelSerializer.Options.JsonSerializerSettings
      );

      if (withConfigurationParameters is not null) {
        var configModel = model as IModel;
        model = configModel.Configure(withConfigurationParameters);
      }

      return (IModel)model;
    }

    /// <summary>
    /// The universe this model was made in
    /// </summary>
    public Universe Universe {
      get;
    }

    /// <summary>
    /// Overrideable static initializer for model classes.
    /// Called right after the static initializer
    /// </summary>
    /// <param name="universe">The current universe being set up</param>
    protected internal static void Setup(Universe universe) { }

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// </summary>
    public IModel Copy() =>
       FromJson(this.ToJson());

    /// <summary>
    /// Can be used to initially configure a model in the base ctor.
    /// Account for a null builder
    /// </summary>
    protected internal IModel Configure(IBuilder builder)
      => this;

    /// <summary>
    /// (optional)Finish deserializing the model
    /// </summary>
    internal protected virtual void FinishDeserialization() {}
  }

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// </summary>
  public partial interface IModel<TModelBase>
    : IModel
    where TModelBase : IModel<TModelBase>
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
      IBuilder withConfigurationParameters = null
     ) => (TModelBase)IModel.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);
  }

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// </summary>
  public interface IModel<TModelBase, TArchetypeBase>
    : IModel<TModelBase>
    where TModelBase : IModel<TModelBase, TArchetypeBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {

    /// <summary>
    /// The archetype for this model
    /// </summary>
    public TArchetypeBase Archetype {
      get;
    }

    /// <summary>
    /// Deserialize a model from json as a TModelBase
    /// </summary>
    /// <param name="deserializeToTypeOverride">You can use this to try to make JsonSerialize 
    ///    use a different Type's info for deserialization than the default returned from GetModelTypeProducedBy</param>
    public new static TModelBase FromJson(
       JObject jObject,
       Type deserializeToTypeOverride = null,
       Universe universeOverride = null,
      IBuilder withConfigurationParameters = null
     ) => IModel<TModelBase>.FromJson(jObject, deserializeToTypeOverride, universeOverride, withConfigurationParameters);
  }

  /// <summary>
  /// Extension methods for models
  /// </summary>
  public static class IModelExtensions {

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    /*public static void Save(this IModel model)
      => throw new System.NotImplementedException();*/

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    public static JObject ToJson(this IModel model, Universe universe = null) {
      var json = JObject.FromObject(
        model, 
        (universe ?? model.Universe)
          .ModelSerializer.JsonSerializer
      );

      return json;
    }

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// Overrideable via IModel.copy()
    /// </summary>
    public static IModel Copy(this IModel original)
      => original.Copy();
  }
}
