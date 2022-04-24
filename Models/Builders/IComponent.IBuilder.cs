using Meep.Tech.Collections.Generic;
using System;
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

      public Builder(Archetype forArchetype, Universe universe = null) 
        : base(forArchetype, universe) {}

      public Builder(Archetype forArchetype, Dictionary<string, object> @params, Universe universe = null) 
        : base(forArchetype, @params, universe) {}

      public Builder(Archetype forArchetype, Dictionary<Param, object> @params, Universe universe = null) 
        : base(forArchetype, @params, universe) {}

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
      public Archetype Archetype {
        get;
      }

      IModel IBuilder.Parent {
        get => Parent;
        set => Parent = value;
      } 
      /// <summary>
      /// If this builder is passed to a child model or component, the original is made the parent.
      /// </summary>
      public IModel Parent {
        get;
        private set;
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

      /// <summary>
      /// The universe this builder is part of
      /// </summary>
      public Universe Universe {
        get;
      }

      public LiteBuilder(Archetype forArchetype, IModel parent = null, Universe universe = null) {
        Archetype = forArchetype;
        InitializeModel =
          builder => (TComponentBase)((IFactory)forArchetype).ModelConstructor(builder);
        ConfigureModel = null;
        FinalizeModel = null;
        @params = null;
        Universe = universe ?? Components.DefaultUniverse;
        Parent = parent;
      }

      public LiteBuilder(Archetype forArchetype, IModel parent, Universe universe, params KeyValuePair<string, object>[] @params)
        : this(forArchetype, parent, universe) {
        this.@params = @params;
      }

      public LiteBuilder(Archetype forArchetype, params KeyValuePair<string, object>[] @params)
        : this(forArchetype) {
        this.@params = @params;
      }

      public LiteBuilder(Archetype forArchetype, IModel parent, params KeyValuePair<string, object>[] @params)
        : this(forArchetype, parent) {
        this.@params = @params;
      }

      public LiteBuilder(Archetype forArchetype, IEnumerable<KeyValuePair<string, object>> @params, IModel parent = null, Universe universe = null)
        : this(forArchetype, parent, universe) {
        this.@params = @params;
      }

      void IBuilder._add(string key, object value) {
        @params = @params.Append(new KeyValuePair<string, object>(key, value));
      }

      bool IBuilder._tryToGetRawValue(string key, out object value) {
        value = @params?.FirstOrDefault(entry => entry.Key == key).Value;
        return !(value is null);
      }

      /// <summary>
      /// Build the model.
      /// </summary>
      public TComponentBase Build() {
        TComponentBase model = InitializeModel(this);
        model = (TComponentBase)(model as IModel).Initialize(this);
        model = (TComponentBase)(model as IModel).Configure(this);

        if(ConfigureModel != null) {
          model = ConfigureModel.Invoke(this, model) ?? model;
        }
        model = (TComponentBase)Archetype.ConfigureModel(this, model);

        if(FinalizeModel != null) {
          model = FinalizeModel(this, model);
        }
        model = (TComponentBase)Archetype.FinalizeModel(this, model);

        return model;
      }

      /// <summary>
      /// Do something with each paramter in the builder.
      /// </summary>
      public void ForEachParam(Action<(string key, object value)> @do)
        => @params.ForEach(entry => @do((entry.Key, entry.Value)));

      TComponentBase IBuilder<TComponentBase>.Build() 
        => Build();

      void IBuilder.ForEachParam(Action<(string key, object value)> @do)
        => ForEachParam(@do);
    }
  }
}