using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial interface IComponent {

    /// <summary>
    /// A simpler, struct based builder for components
    /// </summary>
    public interface ILiteBuilder : IBuilder {

      /// <summary>
      /// The param collection.
      /// </summary>
      public IEnumerable<KeyValuePair<string, object>> @params {
        get;
      }
    }
  }

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent
    where TComponentBase : IComponent<TComponentBase> {

    /// <summary>
    /// Default builder class for components. Pretty much the same as the model based one.
    /// </summary>
    public new class Builder : IModel<TComponentBase>.Builder {

      public Builder(Archetype forArchetype) 
        : base(forArchetype) {}

      public Builder(Archetype forArchetype, Dictionary<string, object> @params) 
        : base(forArchetype, @params) {}

      public Builder(Archetype forArchetype, Dictionary<Param, object> @params) 
        : base(forArchetype, @params) {}

      public Builder(Archetype forArchetype, bool Immutable) 
        : base(forArchetype, Immutable) {}
    }

    /// <summary>
    /// A simpler builder for components
    /// </summary>
    public struct LiteBuilder : IBuilder<TComponentBase>, ILiteBuilder {

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
      public Func<LiteBuilder, TComponentBase> InitializeModel {
        get;
        set;
      }

      /// <summary>
      /// Configure and set param on the empty new model from InitializeModel.
      /// </summary>
      public Func<LiteBuilder, TComponentBase, TComponentBase> ConfigureModel {
        get;
        set;
      }

      /// <summary>
      /// Logic to finish setting up the model.
      /// </summary>
      public Func<LiteBuilder, TComponentBase, TComponentBase> FinalizeModel {
        get;
        set;
      }

      /// <summary>
      /// The param collection.
      /// </summary>
      public IEnumerable<KeyValuePair<string, object>> @params {
        get;
        private set;
      }

      public LiteBuilder(Archetype forArchetype) {
        Type = forArchetype;
        InitializeModel =
          builder => (TComponentBase)forArchetype.ModelConstructor(builder);
        ConfigureModel = null;
        FinalizeModel = null;
        @params = null;
      }

      public LiteBuilder(Archetype forArchetype, params KeyValuePair<string, object>[] @params)
        : this(forArchetype) {
        this.@params = @params;
      }

      public LiteBuilder(Archetype forArchetype, IEnumerable<KeyValuePair<string, object>> @params)
        : this(forArchetype) {
        this.@params = @params;
      }

      void IBuilder._add(string key, object value) {
        @params = @params.Append(new KeyValuePair<string, object>(key, value));
      }

      bool IBuilder._tryToGetRawValue(string key, out object value) {
        value = @params.FirstOrDefault(entry => entry.Key == key);
        return value != null;
      }

      /// <summary>
      /// Build the model.
      /// </summary>
      public TComponentBase Build() {
        TComponentBase model = InitializeModel(this);

        if(ConfigureModel != null) {
          model = ConfigureModel.Invoke(this, model) ?? model;
          model = (TComponentBase)Type.ConfigureModel(this, model);
        }

        if(FinalizeModel != null) {
          model = FinalizeModel(this, model);
          model = (TComponentBase)Type.FinalizeModel(this, model);
        }

        return model;
      }

      public void ForEachParam(Action<(string key, object value)> @do)
        => @params.ForEach(entry => @do((entry.Key, entry.Value)));
    }
  }
}