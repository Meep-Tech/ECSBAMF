using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// This is the non generic for Utility only
  /// </summary>
  public partial interface IModel {

    /// <summary>
    /// Make a component from a jobject
    /// </summary>
    public static IModel FromJson(JObject jObject) {
      string key;
      Universe universe;
      string compoundKey = jObject.Value<string>(nameof(Archetype).ToLower());
      string[] parts = compoundKey.Split('@');
      if(parts.Length == 1) {
        key = compoundKey;
        universe = Models.DefaultUniverse;
      }
      else if(parts.Length == 2) {
        key = parts[0];
        universe = Universe.Get(parts[1]);
      }
      else
        throw new ArgumentException($"No __key_ identifier provided in component data: \n{jObject}");

      string json = jObject.ToString();
      Type deserializeToType =
        universe.Models.GetModelTypeProducedBy(
          universe.Archetypes.All.Get(key)
        );
      object model = JsonConvert.DeserializeObject(
        json,
        deserializeToType,
        universe.ModelSerializer.Options.ModelJsonSerializerSettings
      );

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
    protected internal IModel Configure(IBuilder builder);

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
    where TModelBase : IModel<TModelBase> {
  }

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// </summary>
  public interface IModel<TModelBase, TArchetypeBase>
    : IModel<TModelBase>
    where TModelBase : IModel<TModelBase, TArchetypeBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {}

  public static class IModelExtensions {

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    /*public static Model.SerializedData Serialize(this IModel model)
      => throw new System.NotImplementedException();*/

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
          .ModelSerializer.Options.ModelJsonSerializer
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
