using Meep.Tech.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  /// <summary>
  /// A singleton data store and factory.
  /// </summary>
  public abstract partial class Archetype : IFactory, IReadableComponentStorage, IEquatable<Archetype> {
    internal DelegateCollection<Func<IModel, IBuilder, IModel>>
      _modelAutoBuilderSteps;

    #region Archetype Data Members

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public Identity Id {
      get;
    }

    /// <summary>
    /// The Base Archetype this Archetype derives from.
    /// </summary>
    public abstract Type BaseArchetype {
      get;
    }

    /// <summary>
    /// The Base type of model that this archetype family produces.
    /// </summary>
    public abstract Type ModelBaseType {
      get;
      internal set;
    }

    /// <summary>
    /// The Base type of model that this archetype family produces.
    /// </summary>
    public Type ModelTypeProduced {
      get => _modelTypeProduced ??= ModelBaseType;
      internal set => _modelTypeProduced = value;
    } Type _modelTypeProduced;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    Func<IBuilder, IModel> IFactory.ModelConstructor {
      get;
      set;
    } = null;

    /// <summary>
    /// The System.Type of this Archetype
    /// </summary>
    public Type Type {
      get;
    }

    /// <summary>
    /// If this is an archetype that inherits from Archetype[,] directly.
    /// </summary>
    public bool IsBaseArchetype
      => BaseArchetype?.Equals(null) ?? true;

    /// <summary>
    /// The collection containing this archetype
    /// </summary>
    public Collection TypeCollection {
      get;
      internal set;
    }

    #endregion

    #region Archetype Configuration Settings

    /// <summary>
    /// The initial default components to add to this archetype on it's creation, indexed by their keys. 
    /// You can add values using the '_InitialComponents ??= base.InitialComponents .Append(' syntax.
    /// </summary>
    protected internal virtual Dictionary<string, Archetype.IComponent> InitialComponents {
      get => _InitialComponents ?? new();
    } /** <summary> The backing field used to initialize and override the initail value of InitialComponents. You can this syntax to override or add to the base initial value: '=> _InitialComponents ??= base.InitialComponents.Append(...' </summary> **/
    protected Dictionary<string, Archetype.IComponent> _InitialComponents {
      get => _initialComponents;
      set => _initialComponents = value;
    } Dictionary<string, Archetype.IComponent> _initialComponents;

    /// <summary>
    /// The Archetype components linked to model components
    /// </summary>
    protected internal IEnumerable<Archetype.IComponent> ModelLinkedComponents
      => _modelLinkedComponents;
    internal HashSet<Archetype.IComponent> _modelLinkedComponents
      = new();

    /// <summary>
    /// Components by key, with optional constructors used to set up the default components on a model made by this Archetype,
    /// Usually you'll want to use an Archetype.ILinkedComponent but this is here too for model components. not linked to an archetype component.
    /// If the constructor function is left null, the default component ctor is used.
    /// Override this field by using "_InitialIUnlinkedModelComponents ??= base.InitialUnlinkedModelComponents.Append...." syntax
    /// </summary>
    protected internal virtual Dictionary<string, Func<IBuilder, IModel.IComponent>> InitialUnlinkedModelComponents {
      get => _InitialIUnlinkedModelComponents ?? new();
    } /**<summary> The backing field used to initialize and override InitialIUnlinkedModelComponentConstructors </summary>**/
    protected Dictionary<string, Func<IBuilder, IModel.IComponent>> _InitialIUnlinkedModelComponents {
      get => _initialIUnlinkedModelComponents; set => _initialIUnlinkedModelComponents = value;
    } Dictionary<string, Func<IBuilder, IModel.IComponent>> _initialIUnlinkedModelComponents;

    /// <summary>
    /// If this is true, this Archetype can have it's component collection modified before load by mods and other libraries.
    /// This does not affect the ability to inherit and override InitialComponents for an archetype.
    /// </summary>
    protected internal virtual bool AllowExternalComponentConfiguration
      => true;

    /// <summary>
    /// If this is true, this archetype and children of it can be initialized after the loader has finished.
    /// Be careful with these, it's up to you to maintain singleton patters.
    /// </summary>
    protected internal virtual bool AllowInitializationsAfterLoaderFinalization
      => false;

    /// <summary>
    /// Default params for testing
    /// </summary>
    internal protected virtual Dictionary<string, object> DefaultTestParams {
      get => _defaultTestParams;
      init => _defaultTestParams = value;
    } internal Dictionary<string, object> _defaultTestParams = null;

    /// <summary>
    /// Finish setting this up
    /// </summary>
    protected internal virtual void Finish() {}

    /// <summary>
    /// Try to unload this archetype
    /// </summary>
    protected internal abstract void TryToUnload();

    #endregion

    #region Initialization

    /// <summary>
    /// Make a new archetype
    /// </summary>
    protected Archetype(Identity id) {
      if(id is null) {
        throw new ArgumentNullException("id");
      }

      Id = id;

      if(Id is null) {
        throw new ArgumentException($"Id is null. The passed in ID May not be of the expected type. Expected:{typeof(Identity).FullName}, provided: {id.GetType().FullName}.");
      }

      Type = GetType();
    }

    /// <summary>
    /// helper for getting the builder constructor from the non-generic base class
    /// </summary>
    /// <returns></returns>
    internal protected abstract Func<Archetype, Dictionary<string, object>, IBuilder> GetGenericBuilderConstructor();

    #endregion

    #region Hash and Equality

    ///<summary><inheritdoc/></summary>
    public override int GetHashCode() 
      => Id.GetHashCode();

    ///<summary><inheritdoc/></summary>
    public override bool Equals(object obj) 
      => (obj as Archetype)?.Equals(this) ?? false;

    ///<summary><inheritdoc/></summary>
    public override string ToString() {
      return $"+{Id}+";
    }

    ///<summary><inheritdoc/></summary>
    public bool Equals(Archetype other) 
      => Id.Key == other?.Id.Key;

    ///<summary><inheritdoc/></summary>
    public static bool operator ==(Archetype a, Archetype b)
      => a?.Equals(b) ?? (b is null);

    ///<summary><inheritdoc/></summary>
    public static bool operator !=(Archetype a, Archetype b)
      => !(a == b);


    #endregion

    #region Configuration Helper Functions

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    internal virtual IModel ConfigureModel(IBuilder builder, IModel model)
      => model;

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    internal virtual IModel FinalizeModel(IBuilder builder, IModel model)
      => model;

    #endregion

    #region Make/Model Construction

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal abstract IModel MakeDefault();

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal abstract IModel MakeDefaultWith(IBuilder builder);

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal abstract IModel MakeDefaultWith(Func<IBuilder, IBuilder> builderConfiguration);

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal TDesiredModel Make<TDesiredModel>()
      where TDesiredModel : IModel
        => (TDesiredModel)MakeDefault();

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal TDesiredModel Make<TDesiredModel>(IBuilder builder)
      where TDesiredModel : IModel
        => (TDesiredModel)MakeDefaultWith(builder);

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal IModel Make(IBuilder builder)
        => MakeDefaultWith(builder);

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    protected internal TDesiredModel Make<TDesiredModel>(Func<IBuilder, IBuilder> builderConfiguration)
      where TDesiredModel : IModel
        => (TDesiredModel)MakeDefaultWith(builderConfiguration);

    #endregion

    #region Default Component Implimentations

    /// <summary>
    /// Publicly readable components
    /// </summary>
    public IReadOnlyDictionary<string, Archetype.IComponent> Components
      => _components.ToDictionary(x => x.Key, y => y.Value as Archetype.IComponent);

    /// <summary>
    /// The accessor for the default Icomponents implimentation
    /// </summary>
    Dictionary<string, Data.IComponent> IReadableComponentStorage._componentsByBuilderKey
      => _components;
    
    /// <summary>
    /// The accessor for the default Icomponents implimentation
    /// </summary>
    Dictionary<System.Type, ICollection<Data.IComponent>> IReadableComponentStorage._componentsWithWaitingContracts { get; }
      = new();

    /// <summary>
    /// Internally stored components
    /// </summary>
    Dictionary<string, Data.IComponent> _components {
      get;
    } = new Dictionary<string, Data.IComponent>();

    #region Read

    /// <summary>
    /// Get a component if it exists. Throws if it doesn't
    /// </summary>
    public IComponent GetComponent(string componentKey)
      => (this as IReadableComponentStorage).GetComponent(componentKey) as Archetype.IComponent;

    /// <summary>
    /// Get a component if it exists. Throws if it doesn't
    /// </summary>
    public IComponent GetComponent<TComponent>(string componentKey)
      where TComponent : Archetype.IComponent
        => (this as IReadableComponentStorage).GetComponent(componentKey) as Archetype.IComponent;

    /// <summary>
    /// Get a component if it exists. Throws if it doesn't
    /// </summary>
    public TComponent GetComponent<TComponent>()
      where TComponent : Archetype.IComponent<TComponent>
        => (this as IReadableComponentStorage).GetComponent<TComponent>();

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public bool TryToGetComponent(System.Type componentType, out Archetype.IComponent component) {
      if((this as IReadableComponentStorage).TryToGetComponent(componentType, out Data.IComponent found)) {
        component = found as Archetype.IComponent;
        return true;
      }

      component = null;
      return false;
    }

    /// <summary>
    /// Check if this has a given component by base type
    /// TODO, if this causes a stackoverflow we'll need to use the extensionmethod trick... which may be cleaner anyway lol
    /// </summary>
    public bool HasComponent(System.Type componentType)
      => (this as IReadableComponentStorage).HasComponent(componentType);

    /// <summary>
    /// Check if this has a component matching the given object
    /// TODO, if this causes a stackoverflow we'll need to use the extensionmethod trick... which may be cleaner anyway lol
    /// </summary>
    public bool HasComponent(string componentBaseKey)
      => (this as IReadableComponentStorage).HasComponent(componentBaseKey);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public bool TryToGetComponent(string componentBaseKey, out Archetype.IComponent component) {
      if((this as IReadableComponentStorage).TryToGetComponent(componentBaseKey, out Data.IComponent found)) {
        component = found as Archetype.IComponent;
        return true;
      }

      component = null;
      return false;
    }

    /// <summary>
    /// Check if this has a component matching the given object
    /// </summary>
    public bool HasComponent(Archetype.IComponent componentModel)
      => (this as IReadableComponentStorage).HasComponent(componentModel);

    /// <summary>
    /// Get a component if this has that given component
    /// </summary>
    public bool TryToGetComponent(Archetype.IComponent componentModel, out Archetype.IComponent component) {
      if((this as IReadableComponentStorage).TryToGetComponent(componentModel, out Data.IComponent found)) {
        component = found as Archetype.IComponent;
        return true;
      }

      component = null;
      return false;
    }

    #endregion

    #region Write

    /// <summary>
    /// Add a new component, throws if the component key is taken already
    /// </summary>
    protected void AddComponent(Archetype.IComponent toAdd) {
      if(toAdd is Archetype.IComponent.IIsRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
        throw new System.ArgumentException($"Component of type {toAdd.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
      }

      (this as IReadableComponentStorage).AddComponent(toAdd);
    }

    /// <summary>
    /// replace an existing component
    /// </summary>
    protected void UpdateComponent(Archetype.IComponent toUpdate) {
      (this as IReadableComponentStorage).UpdateComponent(toUpdate);
    }

    /// <summary>
    /// update an existing component, given it's current data
    /// </summary>
    protected void UpdateComponent<TComponent>(System.Func<TComponent, TComponent> UpdateComponent)
      where TComponent : Archetype.IComponent {
      (this as IReadableComponentStorage).UpdateComponent(UpdateComponent);
    }

    /// <summary>
    /// Add or replace a component
    /// </summary>
    protected void AddOrUpdateComponent(Archetype.IComponent toSet) {
      if(toSet is Archetype.IComponent.IIsRestrictedToCertainTypes restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
        throw new System.ArgumentException($"Component of type {toSet.Key} is not compatable with model of type {GetType()}. The model must inherit from {restrictedComponent.RestrictedTo.FullName}.");
      }
      (this as IReadableComponentStorage).AddOrUpdateComponent(toSet);
    }

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent(Archetype.IComponent toRemove)
      => (this as IReadableComponentStorage).RemoveComponent(toRemove.Key);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent<TComponent>()
      where TComponent : Archetype.IComponent<TComponent>
        => (this as IReadableComponentStorage).RemoveComponent<TComponent>();

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent<TComponent>(out IComponent removed)
      where TComponent : Archetype.IComponent<TComponent> {
      if((this as IReadableComponentStorage).RemoveComponent<TComponent>(out Data.IComponent found)) {
        removed = found as Archetype.IComponent;
        return true;
      }

      removed = null;
      return false;
    }

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent(System.Type toRemove)
      => (this as IReadableComponentStorage).RemoveComponent(toRemove);

    /// <summary>
    /// Remove an existing component
    /// </summary>
    protected bool RemoveComponent(System.Type toRemove, out IComponent removed) {
      if((this as IReadableComponentStorage).RemoveComponent(toRemove, out Data.IComponent found)) {
        removed = found as Archetype.IComponent;
        return true;
      }

      removed = null;
      return false;
    }

    /// <summary>
    /// Remove and get an existing component
    /// </summary>
    protected bool RemoveComponent(string componentKeyToRemove, out Archetype.IComponent removedComponent) {
      if((this as IReadableComponentStorage).RemoveComponent(componentKeyToRemove, out Data.IComponent component)) {
        removedComponent = component as Archetype.IComponent;
        return true;
      }

      removedComponent = null;
      return false;
    }

    #endregion

    #endregion
  
    /// <summary>
    /// Used to deserialize a jobject by default.
    /// </summary>
    protected virtual internal IModel DeserializeModelFromJson(JObject jObject, Type deserializeToTypeOverride = null, params (string key, object value)[] withConfigurationParameters) {
      string json = jObject.ToString();
      deserializeToTypeOverride
        ??= Id.Universe.Models.GetModelTypeProducedBy(this);
      IModel model = JsonConvert.DeserializeObject(
        json,
        deserializeToTypeOverride,
        Id.Universe.ModelSerializer.Options.JsonSerializerSettings
      ) as IModel;

      model.Universe = Id.Universe;
      // default init and configure.
      if (withConfigurationParameters.Any()) {
        Archetype builderFactory = (Models.GetBuilderFactoryFor(model.GetType()) as Archetype);
        IBuilder builder = builderFactory
          .GetGenericBuilderConstructor()(builderFactory, withConfigurationParameters.ToDictionary(p => p.key, p => p.value));
        model = model.Initialize(builder);
        model = model.Configure(builder);
      }

      return model;
    }

    /// <summary>
    /// Used to serialize a model with this archetype to a jobject by default
    /// </summary>
    protected internal virtual JObject SerializeModelToJson(IModel model)
      => JObject.FromObject(
        model,
        Id.Universe.ModelSerializer.JsonSerializer
      );
  }

  /// <summary>
  /// An Id unique to each Archetype.
  /// Can be used as a static key.
  /// </summary>
  public abstract partial class Archetype<TModelBase, TArchetypeBase> 
    : Archetype, IFactory
    where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> 
  {

    #region Archetype Data Members

    /// <summary>
    /// The collection containing this archetype
    /// </summary>
    public new Collection TypeCollection 
      => base.TypeCollection as Collection;

    /// <summary>
    /// The base archetype that all of the ones like it are based on.
    /// </summary>
    public sealed override System.Type BaseArchetype
      => typeof(TArchetypeBase);

    /// <summary>
    /// The most basic model that this archetype can produce.d
    /// This is used to generat the default model constructor.
    /// </summary>
    public sealed override System.Type ModelBaseType {
      get => _ModelBaseType;
      internal set => _ModelBaseType = value;
    } System.Type _ModelBaseType
      = typeof(TModelBase);

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public new Identity Id
      => base.Id as Identity;

    /// <summary>
    /// Just used to get Id in case the Id namespace is overriden.
    /// </summary>
    public Identity GetId()
      => Id;

    #endregion

    #region Archetype Initialization

    /// <summary>
    /// The base for making a new archetype.
    /// This should be extended into a private constructor that will only be called once by the Loader
    /// </summary>
    protected Archetype(Archetype.Identity id, Collection collection = null) 
      : base(id) 
    {
      if(collection is null) {
        if (Archetypes.DefaultUniverse.Loader.IsFinished && !AllowInitializationsAfterLoaderFinalization) {
          throw new InvalidOperationException($"Tried to initialize archetype of type {id} while the loader was sealed");
        }

        collection = (Collection)
          // if the base of this is registered somewhere, get the registered one by default
          (Archetypes.DefaultUniverse.Archetypes._tryToGetCollectionFor(GetType(), out var found)
            ? found is Collection
              ? found
              : Archetypes.DefaultUniverse.Archetypes._collectionsByRootArchetype[typeof(TArchetypeBase).FullName]
                = new Collection()
            // else this is the base and we need a new one
            : Archetypes.DefaultUniverse.Archetypes._collectionsByRootArchetype[typeof(TArchetypeBase).FullName] 
              = new Collection());
      } // if we have a collection, just make sure it accepts new entries
      else if (collection.Universe.Loader.IsFinished && !AllowInitializationsAfterLoaderFinalization) {
        throw new InvalidOperationException($"Tried to initialize archetype of type {id} while the loader was sealed");
      }

      collection.Universe.Archetypes._registerArchetype(this, collection);
      _initialize();
    }

    /// <summary>
    /// The base for making a new archetype in a universe other than the default.
    /// </summary>
    protected Archetype(Archetype.Identity id, Universe universe, Collection collection = null) 
      : base(id) 
    {
      if (universe is null) {
        universe = Archetypes.DefaultUniverse;
      }

      if (universe.Loader.IsFinished && !AllowInitializationsAfterLoaderFinalization) {
        throw new InvalidOperationException($"Tried to initialize archetype of type {id} while the loader was sealed");
      }

      if (collection is null) {
        collection = (Collection)
          // if the base of this is registered somewhere, get the registered one by default
          (universe.Archetypes._tryToGetCollectionFor(GetType(), out var found)
            ? found is Collection
              ? found
              : universe.Archetypes._collectionsByRootArchetype[typeof(TArchetypeBase).FullName]
                = new Collection()
            // else this is the base and we need a new one
            : universe.Archetypes._collectionsByRootArchetype[typeof(TArchetypeBase).FullName]
              = new Collection());
      }

      collection.Universe.Archetypes._registerArchetype(this, collection);
      _initialize();
    }

    /// <summary>
    /// Initialize this Archetype internally
    /// </summary>
    void _initialize() {
      _initializeInitialComponents();
    }

    /// <summary>
    /// Add all initial components
    /// </summary>
    void _initializeInitialComponents() {
      foreach(Archetype.IComponent component in InitialComponents.Values) {
        AddComponent(component);
        if(component is Archetype.IComponent.ILinkedComponent linkedComponent) {
          _modelLinkedComponents.Add(linkedComponent);
        }
      }
    }

    /// <summary>
    /// Deinitialize this Archetype internally
    /// </summary>
    void _deInitialize() {
      _modelConstructor = null;
      _deInitializeInitialComponents();
    }

    /// <summary>
    /// remove all components
    /// </summary>
    void _deInitializeInitialComponents() {
      foreach(Archetype.IComponent component in InitialComponents.Values) {
        RemoveComponent(component);
        if(component is Archetype.IComponent.ILinkedComponent linkedComponent) {
          _modelLinkedComponents.Remove(linkedComponent);
        }
      }
    }

    #endregion

    #region Model Construction 

    #region Model Constructor Settings

    /// <summary>
    /// Overrideable Model Construction logic.
    /// Make sure to set overrides using "base.ModelConstructor ??= " in order to maintain auto-builder functionality.
    /// </summary>
    protected internal virtual Func<IBuilder<TModelBase>, TModelBase> ModelConstructor {
      get => _modelConstructor is not null 
        ? (builder) => {
          var model = (TModelBase)_modelConstructor(builder);
          _modelAutoBuilderSteps?.ForEach(a => model = (TModelBase)a.Value(model, builder));
          return DoAfterAutoBuildSteps(model, builder);
        } : null;
      set {
        _modelConstructor
          = builder => value.Invoke(builder);

        IModel model
          = Configuration.Loader.GetOrBuildTestModel(
              this,
              ModelTypeProduced
          );

        // register it
        System.Type constructedModelType = model.GetType();
        ModelTypeProduced = constructedModelType;
        Id.Universe.Archetypes._rootArchetypeTypesByBaseModelType[constructedModelType.FullName]
          = GetType();

        /// add auto builder properties based on the model type:
        _modelAutoBuilderSteps = AutoBuildAttribute._generateAutoBuilderSteps(constructedModelType)
          .ToDictionary(
           e => e.name,
           e => e.function
          );
      }
    }

    /// <summary>
    /// An overrideable function allowing a user to modify a model after auto builder has run.
    /// </summary>
    protected virtual TModelBase DoAfterAutoBuildSteps(TModelBase model, IBuilder<TModelBase> builder)
      => model;

    Func<IBuilder<TModelBase>, IModel> _modelConstructor;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    Func<IBuilder, IModel> IFactory.ModelConstructor {
      get => ModelConstructor is null 
        ? null 
        : builder => ModelConstructor((IBuilder<TModelBase>)builder);
      set => ModelConstructor = builder => (TModelBase)value(builder);
    }

    #endregion

    #region Build/Make

    #region Builder Setup

    /// <summary>
    /// An empty builder used to help build for this archetype:
    /// </summary>
    IBuilder<TModelBase> _defaultEmptyBuilder
      = null;

    /// <summary>
    /// The default way a new builder is created.
    /// The dictionary passed in has the potential to be null
    /// </summary>
    internal protected virtual Func<Archetype, Dictionary<string, object>, Universe, IBuilder<TModelBase>> BuilderConstructor {
      get => _defaultBuilderCtor ??= (archetype, @params, universe) 
        => !(@params is null) 
          ? new IModel<TModelBase>.Builder(archetype, @params, universe)
          : new IModel<TModelBase>.Builder(archetype, universe); 
      set => _defaultBuilderCtor = value;
    } internal Func<Archetype, Dictionary<string, object>, Universe, IBuilder<TModelBase>> _defaultBuilderCtor;

    /// <summary>
    /// helper for getting the builder constructor from the non-generic base class
    /// TODO: I can probably cache this at least.
    /// </summary>
    protected internal sealed override Func<Archetype, Dictionary<string, object>, IBuilder> GetGenericBuilderConstructor()
      => (archetype, @params) => BuilderConstructor(archetype, @params, null);

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    protected virtual IBuilder<TModelBase> MakeDefaultBuilder()
      => (IBuilder<TModelBase>)GetGenericBuilderConstructor()(this, null);

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    protected IBuilder<TModelBase> MakeBuilder(Dictionary<string, object> @params)
      => (IBuilder<TModelBase>)GetGenericBuilderConstructor()(this, @params);

    /// <summary>
    /// Gets an immutable empty builder for this type to use when null was passed in:
    /// </summary>
    protected virtual IBuilder<TModelBase> MakeDefaultEmptyBuilder()
      => MakeDefaultBuilder() is IBuilder<TModelBase> builder
        ? builder is IModel<TModelBase>.Builder objectBasedBuilder
          ? objectBasedBuilder.AsImmutable() as IBuilder<TModelBase>
          : builder
        : default;

    #region Configuration Helper Functions

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    protected internal virtual TModelBase ConfigureModel(IBuilder<TModelBase> builder, TModelBase model)
      => model;

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    protected internal virtual TModelBase FinalizeModel(IBuilder<TModelBase> builder, TModelBase model)
      => model;


    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    internal override IModel ConfigureModel(IBuilder builder, IModel model)
      => ConfigureModel(builder as IBuilder<TModelBase>, (TModelBase)model);

    /// <summary>
    /// Function that gets called by default in builders.
    /// Can be used to add logic during model setup as a shortcut.
    /// </summary>
    internal override IModel FinalizeModel(IBuilder builder, IModel model)
      => FinalizeModel(builder as IBuilder<TModelBase>, (TModelBase)model);


    #endregion

    #endregion

    #region List Based

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// This does by default for models.
    /// </summary>
    /// <returns></returns>
    protected internal virtual TModelBase Make(IEnumerable<KeyValuePair<string, object>> @params)
      => BuildModel(MakeBuilder(@params?.ToDictionary(
        param => param.Key,
        param => param.Value
      )));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(IEnumerable<(string key, object value)> @params)
      => Make(@params?.Select(entry => new KeyValuePair<string,object>(entry.key, entry.value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IEnumerable<(string key, object value)> @params)
      where TDesiredModel : TModelBase
      => (TDesiredModel)Make(@params?.Select(entry => new KeyValuePair<string,object>(entry.key, entry.value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(params (string key, object value)[] @params)
      => Make((IEnumerable<(string key, object value)>)@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(IEnumerable<(IModel.Builder.Param key, object value)> @params)
      => Make(@params?.Select(entry => new KeyValuePair<IModel.Builder.Param, object>(entry.key, entry.value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IEnumerable<(IModel.Builder.Param key, object value)> @params)
      where TDesiredModel : TModelBase
      => (TDesiredModel)Make(@params?.Select(entry => new KeyValuePair<IModel.Builder.Param, object>(entry.key, entry.value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    /*protected internal TModelBase Make(params (IModel.Builder.Param key, object value)[] @params)
      => Make((IEnumerable<(IModel.Builder.Param key, object value)>)@params);*/

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
      => Make(@params?.Select(entry => new KeyValuePair<string,object>(entry.Key.Key, entry.Value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make(@params.Select(entry => new KeyValuePair<string,object>(entry.Key.Key, entry.Value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    /*protected internal TModelBase Make(params KeyValuePair<IModel.Builder.Param, object>[] @params)
      => Make((IEnumerable<KeyValuePair<IModel.Builder.Param, object>>)@params);*/

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    /*protected internal TModelBase Make(params KeyValuePair<string, object>[] @params)
      => Make(@params.AsEnumerable());*/
    
    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    protected internal TDesiredModel Make<TDesiredModel>(params (string key, object value)[] @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make((IEnumerable<(string key, object value)>)@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    protected internal TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<string, object>> @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    /*protected internal TDesiredModel Make<TDesiredModel>(params KeyValuePair<string, object>[] @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make(@params);*/

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    protected internal TDesiredModel MakeAs<TDesiredModel>(IEnumerable<KeyValuePair<string, object>> @params, out TDesiredModel model)
      where TDesiredModel : TModelBase
        => model = (TDesiredModel)Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    protected internal TDesiredModel MakeAs<TDesiredModel>(IEnumerable<(string, object)> @params, out TDesiredModel model)
      where TDesiredModel : class, TModelBase
        => model = (TDesiredModel)Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    /*protected internal TDesiredModel MakeAs<TDesiredModel>(out TDesiredModel model, params KeyValuePair<string, object>[] @params)
      where TDesiredModel : class, TModelBase
        => model = (TDesiredModel)Make(@params);*/

    #endregion

    #region Builder Based

    /// <summary>
    /// Build the model with the builder.
    /// </summary>
    protected internal virtual TModelBase BuildModel(IBuilder<TModelBase> builder = null) {
      var builderToUse = builder;
      if(builder is null) {
        builderToUse = _defaultEmptyBuilder ??= MakeDefaultEmptyBuilder();
      }

      var model = builderToUse.Build();
      if (model.Universe is null) {
        model.Universe = Id.Universe;
      }

      return model;
    }

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    /// <returns></returns>
    protected internal sealed override IModel MakeDefault()
      => Make();

    /// <summary>
    /// Make a default model from this Archetype with the builder
    /// </summary>
    /// <returns></returns>
    protected internal sealed override IModel MakeDefaultWith(Func<IBuilder, IBuilder> builderConfiguration)
      => Make(builderConfiguration);

    /// <summary>
    /// Make a default model from this Archetype with the builder
    /// </summary>
    /// <returns></returns>
    protected internal sealed override IModel MakeDefaultWith(IBuilder builder)
      => Make(builder as IBuilder<TModelBase>);

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    /// <returns></returns>
    protected internal TModelBase Make()
      => BuildModel(null);

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    protected internal new TDesiredModel Make<TDesiredModel>()
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(null);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    protected internal TModelBase Make(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      => BuildModel(configureBuilder(MakeDefaultBuilder()));

    /// <summary>
    /// Make a model by passing in an builder.
    /// </summary>
    protected internal TModelBase Make(IModel<TModelBase>.Builder builder)
      => BuildModel(builder);

    /// <summary>
    /// Make a model by and configuring the default builder.
    /// </summary>
    protected internal TModelBase Make(IBuilder<TModelBase> builder)
      => BuildModel(builder);

    /// <summary>
    /// Make a model that requires a struct based builder:
    /// </summary>
    protected internal TModelBase Make(Action<IModel<TModelBase>.Builder> configureBuilder) {
      IModel<TModelBase>.Builder builder = (IModel<TModelBase>.Builder)MakeDefaultBuilder();
      configureBuilder(builder);

      return BuildModel(builder);
    }

    /// <summary>
    /// Make a model that requires a struct based builder:
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(Action<IModel<TModelBase>.Builder> configureBuilder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make(configureBuilder);

    /// <summary>
    /// Make a model that requires a struct based builder:
    /// </summary>
    protected internal TModelBase Make(Func<IBuilder, IBuilder> configureBuilder)
      => Make(builder => (configureBuilder(MakeDefaultBuilder()) as IBuilder<TModelBase>));


    /// <summary>
    /// Make a model that requires an object based builder:
    /// </summary>
    protected internal TModelBase Make(Action<IModel.Builder> configureBuilder)
      => Make(builder => {
        configureBuilder((IModel.Builder)builder);

        return builder;
      });

    /// <summary>
    /// Make a model that requires an object based builder:
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(Action<IModel.Builder> configureBuilder)
     where TDesiredModel : TModelBase
        => (TDesiredModel)Make(configureBuilder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IModel<TModelBase>.Builder builder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(IBuilder<TModelBase> builder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(configureBuilder((IModel<TModelBase>.Builder)MakeDefaultBuilder()));

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    protected internal TModelBase Make(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder)
      => BuildModel(configureBuilder((IModel<TModelBase>.Builder)MakeDefaultBuilder()));

    /// <summary>
    /// Make a model that requires a struct based builder"
    /// </summary>
    protected internal TDesiredModel Make<TDesiredModel>(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(configureBuilder(MakeDefaultBuilder()));

    #endregion

    #endregion

    #endregion

    #region Archetype DeInitialization

    /// <summary>
    /// Called on unload before the type is actually un-registered from the universe.
    /// the base version of this calls OnUnload for all extra contexts, if there are any.
    /// </summary>
    protected virtual void OnUnloadFrom(Universe universe)
      => universe.ExtraContexts.OnUnloadArchetype(this);

    /// <summary>
    /// Attempts to unload this archetype from the universe and collections it's registered to
    /// </summary>
    protected internal sealed override void TryToUnload() {
      // TODO: should this be it's own setting; AllowDeInitializationsAfterLoaderFinalization?
      if (!Id.Universe.Loader.IsFinished || AllowInitializationsAfterLoaderFinalization) {
        Universe universe = Id.Universe;
        OnUnloadFrom(universe);
        universe.Archetypes._unRegisterArchetype(this);
        Id._deRegisterFromCurrentUniverse();
        _deInitialize();
      }
    }

    #endregion
  }
}
