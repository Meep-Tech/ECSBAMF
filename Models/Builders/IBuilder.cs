using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base interface for builders
  /// </summary>
  public partial interface IBuilder : IEnumerable<KeyValuePair<string, object>>, IReadOnlyDictionary<string, object> {

    /// <summary>
    /// The universe this is being built in
    /// </summary>
    Universe Universe {
      get;
    }
    
    /// <summary>
    /// The archetype that initialize the building and made the builder
    /// </summary>
    Archetype Archetype {
      get;
    }

    /// <summary>
    /// The parameters contained in this builder as a list.
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Parameters {
      get;
    }

    /// <summary>
    /// Return the builder with a new value appended.
    /// </summary>
    IBuilder Append(KeyValuePair<string, object> parameter);

    /// <summary>
    /// Return the builder with a new value appended.
    /// </summary>
    IBuilder Append(string key, object value)
      => Append(new KeyValuePair<string, object>(key, value));

    /// <summary>
    /// Try to get a parameter by the key 
    /// </summary>
    object Get(string key);

    /// <summary>
    /// Try to get a parameter by the key 
    /// </summary>
    bool TryToGet(string key, out object value);

    /// <summary>
    /// See if there's a param with this key
    /// </summary>
    bool Has(string key);

    /// <summary>
    /// Execute a builder
    /// </summary>
    IModel Make();
  }

  /// <summary>
  /// The base interface for builders
  /// </summary>
  public interface IBuilder<TModelBase>
    : IBuilder
    where TModelBase : IModel<TModelBase> 
  {
/*
    /// <summary>
    /// used by a builder to initialize it's model.
    /// Uses IFactory.ModelConstructor(builder): by default.
    /// </summary>
    Func<IModel<TModelBase>.Builder, TModelBase> InitializeModel
      => builder => (TModelBase)((IFactory)builder.Archetype).ModelConstructor(builder);

    /// <summary>
    /// Used by a builder to configure it's model
    /// </summary>
    Func<IModel<TModelBase>.Builder, TModelBase, TModelBase> ConfigureModel
      => null;

    /// <summary>
    /// Used by a builder to finalize it's model
    /// </summary>
    Func<IModel<TModelBase>.Builder, TModelBase, TModelBase> FinalizeModel
      => null;*/

    /// <summary>
    /// Return the builder with a new value appended.
    /// </summary>
    new IBuilder<TModelBase> Append(KeyValuePair<string, object> parameter);
    IBuilder IBuilder.Append(KeyValuePair<string, object> parameter)
      => Append(parameter);

    /// <summary>
    /// Return the builder with a new value appended.
    /// </summary>
    new IBuilder<TModelBase> Append(string key, object value)
      => Append(new KeyValuePair<string, object>(key, value));
    IBuilder IBuilder.Append(string key, object value)
      => Append(key, value);

    /// <summary>
    /// Execute a builder
    /// </summary>
    new TModelBase Make();

    IModel IBuilder.Make()
      => Make();
  }
}