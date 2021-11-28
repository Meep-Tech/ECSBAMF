﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Model {

    /// <summary>
    /// A modifyable parameter container that is used to build a model.
    /// The non-generic base class for utility.
    /// </summary>
    public abstract partial class Builder : Dictionary<string, object>, IBuilder {

      /// <summary>
      /// If this builder can be modified
      /// </summary>
      internal bool _isImmutable;

      /// <summary>
      /// Empty new builder
      /// </summary>
      protected Builder(bool Immutable = false) 
        : base() {_isImmutable = Immutable; }

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      protected Builder(Dictionary<string, object> @params) : base(@params) { }

      /// <summary>
      /// New builder from a collection of params
      /// </summary>
      protected Builder(Dictionary<Param, object> @params) : base(@params.ToDictionary(
        param => param.Key.Key,
        param => param.Value
      )) { }

      #region Data Access

      public object this[Param param] {
        get => this[param.Key];
        set {
          if(_isImmutable) {
            throw new AccessViolationException($"Cannot change params on an immutable builder");
          }
          this[param.Key] = value;
        }
      }

      public new object this[string param] {
        get => this[param];
        set {
          if(_isImmutable) {
            throw new AccessViolationException($"Cannot change params on an immutable builder");
          }
          this[param] = value;
        }
      }

      void IBuilder._add(string key, object value) 
        => Add(key, value);

      bool IBuilder._tryToGetRawValue(string key, out object value)
        => TryGetValue(key, out value);

      /// <summary>
      /// Add override
      /// </summary>
      /// <param name="key"></param>
      /// <param name="value"></param>
      public new void Add(string key, object value) 
        => this.set(key, value);

      #endregion 
      
      /// <summary>
      /// Just make this immutible.
      /// </summary>
      internal Builder asImmutable() {
        _isImmutable = true;
        return this;
      }

      public void forEachParam(Action<(string key, object value)> @do) 
        => this.ForEach(entry => @do((entry.Key, entry.Value)));

      /// <summary>
      /// Can be used for preventing a mappable type from retries while being built
      /// </summary>
      public class DoNotBuildThisTypeException : InvalidOperationException {
        public DoNotBuildThisTypeException(string message) : base(message) { }
      }
    }
  }

  public partial class Model<TModelBase> 
    where TModelBase : IModel<TModelBase> 
  {

    /// <summary>
    /// A modifyable parameter container that is used to build a model.
    /// </summary>
    public new partial class Builder : Model.Builder, IBuilder<TModelBase> {

      /// <summary>
      /// The archetype/factory using this builder.
      /// </summary>
      public Archetype Type {
        get;
      }

      /// <summary>
      /// Empty new builder
      /// </summary>
      internal protected Builder(Archetype forArchetype)
        : base() {
        Type = forArchetype;
      }

      /// <summary>
      /// Empty new builder, immutable, for internal use only
      /// </summary>
      internal Builder(Archetype forArchetype, bool Immutable)
        : base(Immutable) {
        Type = forArchetype;
      }

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      /// 
      internal protected Builder(Archetype forArchetype, Dictionary<string, object> @params)
        : base(@params) {
        Type = forArchetype;
      }

      /// <summary>
      /// New builder from a collection of params
      /// </summary>
      internal protected Builder(
        Archetype forArchetype,
        Dictionary<Param, object> @params
      ) : base(@params.ToDictionary(
        param => param.Key.Key,
        param => param.Value
      )) {
        Type = forArchetype;
      }

      /// <summary>
      /// Produce a new instance of the model type.
      /// this usually is just calling => new Model(this) to help set the type variable or something.
      /// </summary>
      public virtual Func<Builder, TModelBase> initializeModel {
        get;
        set;
      } = builder => (TModelBase)builder.Type.ModelConstructor(builder);

      /// <summary>
      /// Configure and set param on the empty new model from InitializeModel.
      /// </summary>
      public virtual Func<Builder, TModelBase, TModelBase> configureModel {
        get;
        set;
      } = (_, model) => model;

      /// <summary>
      /// Logic to finish setting up the model.
      /// </summary>
      public virtual Func<Builder, TModelBase, TModelBase> finalizeModel {
        get;
        set;
      } = (_, model) => model;

      /// <summary>
      /// Build the model.
      /// </summary>
      public TModelBase build() {
        TModelBase model = initializeModel(this);

        /// Unique ID logic:
        /// TOOD: move this to IUnique somehow
        if(model is IUnique unique) {
          if(this.tryToGet(IUnique.Params.UniqueId, out string providedId)) {
            unique.id = providedId;
          }
          else if(unique.id == null) {
            unique.id = RNG.GetNextUniqueId();
          }
        }

        model = configureModel(this, model);

        IComponentStorage componentStorage = model as IComponentStorage;
        if(componentStorage != null) {
          model = _initializeModelComponents(model);
        }

        // finalize the parent model
        model = finalizeModel(this, model);

        // finalize the child components:
        if(componentStorage != null) {
          model = _finalizeModelComponents(model);
        }

        // cache the model if it's cacheable
        if(model is ICached modelToCache) {
          ICached.Cache(modelToCache);
        }

        return model;
      }

      /// <summary>
      /// Loop though each model component and initialize them.
      /// This also adds all model data componnets linked to an archetype component first.
      /// </summary>
      protected TModelBase _initializeModelComponents(TModelBase model)
        => throw new NotImplementedException();

      /// <summary>
      /// Loop though each model component and finalize them.
      /// </summary>
      protected TModelBase _finalizeModelComponents(TModelBase model)
        => throw new NotImplementedException();
    }
  }
}