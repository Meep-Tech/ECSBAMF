﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// This is the non-generic for utility reasons.
  /// </summary>
  public interface IComponent : IModel {

    /// <summary>
    /// Access to the builder factory for this type of component
    /// </summary>
    public IComponent.IBuilderFactory Factory
      => Components.GetBuilderFactoryFor(GetType());

    /// <summary>
    /// Access to the builder factory for this type of component
    /// </summary>
    string Key
      => Factory.Key;

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// This is the base interface.
    /// </summary>
    public interface IBuilderFactory 
      : Model.IBuilderFactory 
    {

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
  public interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent 
    where TComponentBase : IComponent<TComponentBase>  
  {

    public class BuilderFactory 
      : Model<TComponentBase>.BuilderFactory,
      IComponent.IBuilderFactory 
    {

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

    /// <summary>
    /// A simpler builder for components
    /// </summary>
    public struct Builder : IBuilder<TComponentBase> {

      /// <summary>
      /// The factory this is for.
      /// </summary>
      public Archetype Type {
        get;
      }

      /// <summary>
      /// Produce a new instance of the model type.
      /// this usually is just calling => new Model(this) to help set the type variable or something.
      /// </summary>
      public Func<Builder, TComponentBase> initializeModel {
        get;
        set;
      }

      /// <summary>
      /// Configure and set param on the empty new model from InitializeModel.
      /// </summary>
      public Func<Builder, TComponentBase, TComponentBase> configureModel {
        get;
        set;
      }

      /// <summary>
      /// Logic to finish setting up the model.
      /// </summary>
      public Func<Builder, TComponentBase, TComponentBase> finalizeModel {
        get;
        set;
      }

      /// <summary>
      /// The param collection.
      /// </summary>
      IEnumerable<KeyValuePair<string, object>> _params;

      public Builder(Archetype forArchetype) {
        Type = forArchetype;
        initializeModel = 
          builder => (TComponentBase)forArchetype.ModelConstructor(builder);
        configureModel = null;
        finalizeModel = null;
        _params = null;
      }

      public Builder(Archetype forArchetype, params KeyValuePair<string, object>[] @params)
        : this(forArchetype) {
        _params = @params;
      }

      public Builder(Archetype forArchetype, IEnumerable<KeyValuePair<string, object>> @params)
        : this(forArchetype) {
        _params = @params;
      }

      void IBuilder._add(string key, object value) {
        _params = _params.Append(new KeyValuePair<string, object>(key, value));
      }

      bool IBuilder._tryToGetRawValue(string key, out object value) {
        value = _params.FirstOrDefault(entry => entry.Key == key);
        return value != null;
      }

      /// <summary>
      /// Build the model.
      /// </summary>
      public TComponentBase build() {
        TComponentBase model = initializeModel(this);

        if(configureModel != null) {
          model = configureModel.Invoke(this, model) ?? model;
        }

        if(finalizeModel != null) {
          model = finalizeModel(this, model);
        }

        return model;
      }

      public void forEachParam(Action<(string key, object value)> @do)
        => _params.ForEach(entry => @do((entry.Key, entry.Value)));
    }
  }
}
