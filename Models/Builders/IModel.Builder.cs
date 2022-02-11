using Meep.Tech.Collections.Generic;
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
      public Archetype Archetype {
        get;
      }

      /// <summary>
      /// The universe this builder is building in
      /// </summary>
      public Universe Universe {
        get;
      }

      /// <summary>
      /// If this builder can be modified
      /// </summary>
      internal bool _isImmutable;

      /// <summary>
      /// Empty new builder
      /// </summary>
      protected Builder(Archetype type, bool Immutable, Universe universe = null) 
        : base() {
        Archetype = type;
        Universe = universe ?? type.Id.Universe;
        _isImmutable = Immutable;
      }

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      protected Builder(Archetype type, Dictionary<string, object> @params, Universe universe = null) 
        : base(@params) {
        Universe = universe;
        Archetype = type;
      }

      /// <summary>
      /// New builder from a collection of params
      /// </summary>
      protected Builder(Archetype type, Dictionary<Param, object> @params, Universe universe = null) 
        : this(type, @params.ToDictionary(
          param => param.Key.Key,
          param => param.Value
        ), universe) { }

      /// <summary>
      /// Copy a builder for a new target type
      /// </summary>
      public static IBuilder MakeNewBuilderAndCopyParams(IBuilder original, Archetype targetFactory) {
        if(original == null) {
          return null;
        }

        return targetFactory.GetGenericBuilderConstructor()(
          targetFactory,
          original is Dictionary<string, object> dictionary 
            ? dictionary 
            : original is Data.IComponent.ILiteBuilder liteBuilder
              ? liteBuilder.@params.ToDictionary(e => e.Key, e => e.Value)
              : throw new NotSupportedException()
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
        => base.Add(key, value);

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

      /// <summary>
      /// Do something with each paramter in the builder.
      /// </summary>
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
      public Builder(Archetype forArchetype, Universe universe = null)
        : base(forArchetype, false, universe) {}

      /// <summary>
      /// Empty new builder, immutable, for internal use only
      /// </summary>
      public Builder(Archetype forArchetype, bool immutable, Universe universe = null)
        : base(forArchetype, immutable, universe) {}

      /// <summary>
      /// New builder from a collection of param names
      /// </summary>
      public Builder(Archetype forArchetype, Dictionary<string, object> @params, Universe universe = null)
        : base(forArchetype, @params, universe) {}

      /// <summary>
      /// New builder from a collection of params
      /// </summary>
      public Builder(
        Archetype forArchetype,
        Dictionary<Param, object> @params,
        Universe universe = null
      ) : base(forArchetype, @params.ToDictionary(
        param => param.Key.Key,
        param => param.Value
      ), universe) { }

      /// <summary>
      /// Produce a new instance of the model type.
      /// this usually is just calling => new Model(this) to help set the type variable or something.
      /// </summary>
      public virtual Func<Builder, TModelBase> InitializeModel {
        get;
        set;
      } = builder => (TModelBase)((IFactory)builder.Archetype).ModelConstructor(builder);

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
        model = (TModelBase)(model as IModel).Configure(this);

        /// Unique ID logic:
        /// TOOD: move this to IUnique somehow
        if(model is IUnique unique && unique.AutoSetIdOnBuild && unique.Id is null) {
          if(this.TryToGetParam(IUnique.Params.UniqueId, out string providedId)) {
            unique.Id = providedId;
          }
          else if(unique.Id == null) {
            unique._resetUniqueId();
          }
        }

        model = ConfigureModel(this, model);
        model = (TModelBase)Archetype.ConfigureModel(this, model);

        IReadableComponentStorage componentStorage = model as IReadableComponentStorage;
        if(componentStorage != null) {
          model = _initializeModelComponents(componentStorage);
        }

#if DEBUG
        // Warns you if you've got Model Component settings in the Archetype but no Component Storage on the Model.
        else if(Archetype.InitialUnlinkedModelComponentCtors.Any() || Archetype.InitialUnlinkedModelComponentTypes.Any() || Archetype.ModelLinkedComponents.Any()) {
          Console.WriteLine($"The Archetype of Type: {Archetype}, provides components to set up on the produced model of type :{model.GetType()}, but this model does not inherit from the interface {nameof(IReadableComponentStorage)}. Maybe consider adding .WithComponents to the Model<[,]> base class you are inheriting from, or removing model components added to any of the Initial[Component...] properties of the Archetype");
#warning An archetype with a Model Base Type that does not inherit from IReadableComponentStorage has been provided with InitialUnlinkedModelComponentCtors values. These components may never be applied to the desired model if it does not inhert from IReadableComponentStorage
        }
#endif

        // finalize the parent model
        model = FinalizeModel(this, model);
        model = (TModelBase)Archetype.FinalizeModel(this, model);

        // finalize the child components:
        if(componentStorage != null) {
          model = _finalizeModelComponents(componentStorage);
        }

        // cache the model if it's cacheable
        if(model is ICached modelToCache) {
          ICached._cacheItem(modelToCache);
        }

        return model;
      }

      /// <summary>
      /// Loop though each model component and initialize them.
      /// This also adds all model data componnets linked to an archetype component first.
      /// </summary>
      protected TModelBase _initializeModelComponents(IReadableComponentStorage model) {
        foreach(Type componentType in Archetype.InitialUnlinkedModelComponentTypes) {
          // Make a builder to match this component with the params from the parent:
          IBuilder componentBuilder
            = MakeNewBuilderAndCopyParams(
                this,
                (Archetype)Components.GetBuilderFactoryFor(componentType)
            );

          // build the component:
          IModel.IComponent component = (Data.Components.GetBuilderFactoryFor(componentType) as Archetype)
            .MakeDefaultWith(componentBuilder) as IModel.IComponent;

          model.AddComponent(component);
        }

        // add components built from a given ctor
        foreach(Func<IBuilder, IModel.IComponent> ctor in Archetype.InitialUnlinkedModelComponentCtors) {
          model.AddComponent(ctor(this)); 
        }

        /// add link components from the archetype
        foreach(Archetype.ILinkedComponent linkComponent in Archetype.ModelLinkedComponents) {
          model.AddComponent(linkComponent.BuildDefaultModelComponent(this, Archetype.Id.Universe));
        }

        return (TModelBase)model;
      }

      /// <summary>
      /// Loop though each model component and finalize them.
      /// </summary>
      protected TModelBase _finalizeModelComponents(IReadableComponentStorage model) {
        foreach(IModel.IComponent component in model._componentsByBuilderKey.Values) {
          component.FinalizeComponentAfterParent(model as IModel);
        }

        return (TModelBase)model;
      }
    }
  }
}