using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial interface IModel {

    /// <summary>
    /// A modifyable parameter container that is used to build a model.
    /// The non-generic base class for utility.
    /// </summary>
    public abstract partial class Builder : Dictionary<string, object>, IBuilder {

      /// <summary>
      /// The archetype/factory using this builder.
      /// </summary>
      public Archetype Type {
        get;
      }

      /// <summary>
      /// If this builder can be modified
      /// </summary>
      internal bool _isImmutable;

      /// <summary>
      /// Empty new builder
      /// </summary>
      protected Builder(Archetype type, bool Immutable = false) 
        : base() {
        Type = type;
        _isImmutable = Immutable;
      }

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      protected Builder(Archetype type, Dictionary<string, object> @params) 
        : base(@params) {
        Type = type;
      }

      /// <summary>
      /// New builder from a collection of params
      /// </summary>
      protected Builder(Archetype type, Dictionary<Param, object> @params) 
        : this(type, @params.ToDictionary(
          param => param.Key.Key,
          param => param.Value
        )) { }

      /// <summary>
      /// Copy a builder for a new target type
      /// </summary>
      public static IBuilder MakeNewBuilderAndCopyParams(IBuilder original, Type targetBuilderFactoryType) {
        if(original == null) {
          return null;
        }

        Archetype targetFactory = targetBuilderFactoryType.AsArchetype();
        return targetFactory.GetGenericBuilderConstructor()(
          targetFactory,
          original is Dictionary<string, object> dictionary 
            ? dictionary 
            : original is Data.IComponent.ILiteBuilder liteBuilder
              ? liteBuilder.@params.ToDictionary(e => e.Key, e => e.Value)
              : throw new ArgumentException()
        );
      } 

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
        => this.SetParam(key, value);

      #endregion 
      
      /// <summary>
      /// Just make this immutible.
      /// </summary>
      internal Builder AsImmutable() {
        _isImmutable = true;
        return this;
      }

      public void ForEachParam(Action<(string key, object value)> @do) 
        => this.ForEach(entry => @do((entry.Key, entry.Value)));

      /// <summary>
      /// Can be used for preventing a mappable type from retries while being built
      /// </summary>
      public class DoNotBuildThisTypeException : InvalidOperationException {
        public DoNotBuildThisTypeException(string message) : base(message) { }
      }
    }
  }

  public partial interface IModel<TModelBase> 
    where TModelBase : IModel<TModelBase> 
  {

    /// <summary>
    /// A modifyable parameter container that is used to build a model.
    /// </summary>
    public new partial class Builder : IModel.Builder, IBuilder<TModelBase> {

      /// <summary>
      /// Empty new builder
      /// </summary>
      public Builder(Archetype forArchetype)
        : base(forArchetype) {}

      /// <summary>
      /// Empty new builder, immutable, for internal use only
      /// </summary>
      public Builder(Archetype forArchetype, bool Immutable)
        : base(forArchetype, Immutable) {}

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      public Builder(Archetype forArchetype, Dictionary<string, object> @params)
        : base(forArchetype, @params) {}

      /// <summary>
      /// New builder from a collection of params
      /// </summary>
      public Builder(
        Archetype forArchetype,
        Dictionary<Param, object> @params
      ) : base(forArchetype, @params.ToDictionary(
        param => param.Key.Key,
        param => param.Value
      )) { }

      /// <summary>
      /// Produce a new instance of the model type.
      /// this usually is just calling => new Model(this) to help set the type variable or something.
      /// </summary>
      public virtual Func<Builder, TModelBase> InitializeModel {
        get;
        set;
      } = builder => (TModelBase)builder.Type.ModelConstructor(builder);

      /// <summary>
      /// Configure and set param on the empty new model from InitializeModel.
      /// </summary>
      public virtual Func<Builder, TModelBase, TModelBase> ConfigureModel {
        get;
        set;
      } = (_, model) => model;

      /// <summary>
      /// Logic to finish setting up the model.
      /// </summary>
      public virtual Func<Builder, TModelBase, TModelBase> FinalizeModel {
        get;
        set;
      } = (_, model) => model;

      /// <summary>
      /// Build the model.
      /// </summary>
      public TModelBase Build() {
        TModelBase model = InitializeModel(this);

        /// Unique ID logic:
        /// TOOD: move this to IUnique somehow
        if(model is IUnique unique) {
          if(this.TryToGetParam(IUnique.Params.UniqueId, out string providedId)) {
            unique.Id = providedId;
          }
          else if(unique.Id == null) {
            unique.Id = RNG.GetNextUniqueId();
          }
        }

        model = ConfigureModel(this, model);
        model = (TModelBase)Type.ConfigureModel(this, model);

        IReadableComponentStorage componentStorage = model as IReadableComponentStorage;
        if(componentStorage != null) {
          model = _initializeModelComponents(model);
        }

        // finalize the parent model
        model = FinalizeModel(this, model);
        model = (TModelBase)Type.FinalizeModel(this, model);

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