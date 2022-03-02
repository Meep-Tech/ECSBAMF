using Newtonsoft.Json.Linq;
using System;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// This is the non-generic for utility reasons.
  /// </summary>
  public partial interface IComponent : IModel {

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public new Universe Universe {
      get;
      protected set;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    Universe IModel.Universe {
      get => Universe;
    }

    /// <summary>
    /// Make a component from a jobject
    /// </summary>
    public new static Data.IComponent FromJson(
      JObject jObject,
      Type deserializeToTypeOverride = null,
      Universe universeOverride = null,
      IBuilder withConfigurationParameters = null
    ) {
      string key;
      Universe universe = universeOverride;
      string compoundKey = jObject.Value<string>(Model.Serializer.ComponentKeyPropertyName);
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
        throw new ArgumentException($"No Archetype identifier provided in component data: \n{jObject}");

      // deserialize a collection type component
      if(jObject.TryGetValue(Model.Serializer.ComponentValueCollectionPropertyName, out JToken valueCollection)) {
        return (IComponent)valueCollection.ToObject(
          deserializeToTypeOverride ?? universe.Components.Get(key),
          universe.ModelSerializer.JsonSerializer
        );
      }

      IComponent component = (IComponent)jObject.ToObject(
        deserializeToTypeOverride ?? universe.Components.Get(key),
        universe.ModelSerializer.JsonSerializer
      );

      if (withConfigurationParameters is not null) {
        component = (IComponent)component.Configure(withConfigurationParameters);
      }

      return component;
    }

    internal static void SetUniverse(ref Data.IComponent component, Universe universe) {
      component.Universe = universe;
    }

    /// <summary>
    /// Access to the builder factory for this type of component
    /// </summary>
    public Data.IComponent.IBuilderFactory Factory
      => (Universe ?? Components.DefaultUniverse).Components.GetBuilderFactoryFor(GetType());

    /// <summary>
    /// A key used to index this type of component. There can only be one componet per key on a model or archetype
    /// This is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
    /// </summary>
    string Key
      => Factory.Key;

    /// <summary>
    /// Default configuration
    /// </summary>
    IModel IModel.Configure(IBuilder builder)
      => this;

    /// <summary>
    /// optional finalization logic for components after the model has been finalized
    /// </summary>
    public Data.IComponent FinalizeComponentAfterParent(IModel parent, IBuilder builder)
      => this;
  }

  /// <summary>
  /// The base interface for components without branching archet
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent 
    where TComponentBase : IComponent<TComponentBase> 
  {
  }

  /// <summary>
  /// Serialization Related Component Extensions
  /// </summary>
  public static class ComponentExtensions {

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    public static JObject ToJson(this IComponent component, Universe universe = null) {
      var json = JToken.FromObject(component, (universe ?? component.Universe).ModelSerializer.JsonSerializer);
      if(json is JObject jsonObject) {
        jsonObject.Add(Model.Serializer.ComponentKeyPropertyName, component.Key);
        return jsonObject;
      } else
        if(json is JArray jsonArray) 
          return new JObject {
            {Model.Serializer.ComponentValueCollectionPropertyName, jsonArray },
            {Model.Serializer.ComponentKeyPropertyName, component.Key }
          };
      else
        throw new NotImplementedException($"Component of type {component.Key} must be serializable to a JObject or JArray by default using it's Universe.ModelSerializer.ComponentJsonSerializer.");
    }

    /// <summary>
    /// Helper function to fetch the key for this component type
    /// A key used to index this type of component. There can only be one componet per key on a model or archetype
    /// The Key is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
    /// </summary>
    public static string GetKey(this IComponent component)
    => component.Key;
  }
}
