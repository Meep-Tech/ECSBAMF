using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  /// <summary>
  /// A singleton data store and factory.
  /// </summary>
  public abstract partial class Archetype : IFactory, IReadableComponentStorage, IEquatable<Archetype> {

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
    public abstract Type BaseType {
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
    /// If this is an archetype that inherits from Archetype<> directly.
    /// </summary>
    public bool IsBase
      => BaseType?.Equals(null) ?? true;

    #endregion

    #region Archetype Configuration Settings

    /// <summary>
    /// The initial default components to add to this archetype on it's creation.
    /// </summary>
    public virtual HashSet<Archetype.IComponent> InitialComponents {
      get;
      init;
    } = new HashSet<IComponent>();

    /// <summary>
    /// The Archetype components linked to model components
    /// </summary>
    public IEnumerable<Archetype.IComponent> ModelLinkedComponents
      => _modelLinkedComponents;
    internal HashSet<Archetype.IComponent> _modelLinkedComponents
      = new HashSet<IComponent>();

    /// <summary>
    /// constructors to make default components on a model made by this Archetype,
    /// Usually you'll want to use an Archetype.ILinkedComponent but this is here too.
    /// </summary>
    public virtual HashSet<Func<IBuilder, IModel.IComponent>> InitialUnlinkedModelComponentCtors {
      get;
      init;
    } = new HashSet<Func<IBuilder, IModel.IComponent>>();

    /// <summary>
    /// The default component types to initialize with default values on a new model made by this archetype
    /// Usually you'll want to use an Archetype.ILinkedComponent but this is here too.
    /// </summary>
    public virtual HashSet<System.Type> InitialUnlinkedModelComponentTypes {
      get;
      init;
    } = new HashSet<System.Type>();

    /// <summary>
    /// If this is true, this Archetype can have it's component collection modified before load by mods and other libraries.
    /// This does not affect the ability to inherit and override InitialComponents for an archetype.
    /// </summary>
    public virtual bool AllowExternalComponentConfiguration
      => true;

    /// <summary>
    /// If this is true, this archetype and children of it can be initialized after the loader has finished.
    /// Be careful with these, it's up to you to maintain singleton patters.
    /// </summary>
    public virtual bool AllowInitializationsAfterLoaderFinalization
      => false;

    /// <summary>
    /// Default params for testing
    /// </summary>
    internal protected virtual Dictionary<string, object> DefaultTestParams {
      get;
      init;
    } = new Dictionary<string, object>();

    /// <summary>
    /// Finish setting this up
    /// </summary>
    protected internal virtual void Finish() {}

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

    public override int GetHashCode() 
      => Id.GetHashCode();

    public override bool Equals(object obj) 
      => (obj as Archetype)?.Equals(this) ?? false;

    public override string ToString() {
      return $"+{Id}+";
    }

    public bool Equals(Archetype other) 
      => Id.Key == other?.Id.Key;

    public static bool operator ==(Archetype a, Archetype b)
      => a?.Equals(b) ?? (b is null);

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
      where TComponent : IModel.IComponent
        => (this as IReadableComponentStorage).GetComponent(componentKey) as Archetype.IComponent;

    /// <summary>
    /// Get a component if this has a component of that given type
    /// </summary>
    public bool HasComponent(System.Type componentType, out Archetype.IComponent component) {
      if((this as IReadableComponentStorage).HasComponent(componentType, out Data.IComponent found)) {
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
    public bool HasComponent(string componentBaseKey, out Archetype.IComponent component) {
      if((this as IReadableComponentStorage).HasComponent(componentBaseKey, out Data.IComponent found)) {
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
    public bool HasComponent(Archetype.IComponent componentModel, out Archetype.IComponent component) {
      if((this as IReadableComponentStorage).HasComponent(componentModel, out Data.IComponent found)) {
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
      if(toAdd is IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
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
      if(toSet is IRestrictedComponent restrictedComponent && !restrictedComponent.IsCompatableWith(this)) {
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
    /// The base archetype that all of the ones like it are based on.
    /// </summary>
    public override System.Type BaseType
      => typeof(TArchetypeBase);

    /// <summary>
    /// The most basic model that this archetype can produce.d
    /// This is used to generat the default model constructor.
    /// </summary>
    public override System.Type ModelBaseType {
      get => _ModelBaseType;
      internal set => _ModelBaseType = value;
    } System.Type _ModelBaseType
      = typeof(TModelBase);

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public new Identity Id
      => base.Id as Identity;

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
      }

      if(collection.Universe.Loader.IsFinished && !AllowInitializationsAfterLoaderFinalization) {
        throw new InvalidOperationException($"Tried to initialize archetype of type {id} while the loader was sealed");
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
      foreach(Archetype.IComponent component in InitialComponents) {
        AddComponent(component);
        if(component is Archetype.ILinkedComponent linkedComponent) {
          _modelLinkedComponents.Add(linkedComponent);
        }
      }
    }

    #endregion

    #region Model Construction 

    #region Model Constructor Settings

    /// <summary>
    /// Overrideable Model Constructor
    /// </summary>
    public virtual Func<IBuilder<TModelBase>, TModelBase> ModelConstructor {
      get {
        if(_modelConstructor == null) {
          _modelConstructor
            = (builder) => Id.Universe.Models._getDefaultCtorFor(ModelBaseType)
              .Invoke(builder);
        }

        return (builder) 
          => (TModelBase)_modelConstructor(builder);
      }
      protected internal set {
        _modelConstructor
          = builder => value.Invoke(builder);

        Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor
          = GetGenericBuilderConstructor();
        IBuilder builder = builderCtor.Invoke(
          this,
          DefaultTestParams
        );

        Id.Universe.Archetypes._rootArchetypeTypesByBaseModelType[_modelConstructor(
         (IBuilder<TModelBase>)builder
        ).GetType().FullName]
          = GetType();
      }
    } Func<IBuilder<TModelBase>, IModel> _modelConstructor;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    Func<IBuilder, IModel> IFactory.ModelConstructor {
      get => builder => ModelConstructor((IBuilder<TModelBase>)builder);
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
    public virtual IBuilder<TModelBase> MakeDefaultBuilder()
      => (IBuilder<TModelBase>)GetGenericBuilderConstructor()(this, null);

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    public IBuilder<TModelBase> MakeBuilder(Dictionary<string, object> @params)
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
      => BuildModel(MakeBuilder(@params.ToDictionary(
        param => param.Key,
        param => param.Value
      )));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(IEnumerable<(string key, object value)> @params)
      => Make(@params.Select(entry => new KeyValuePair<string,object>(entry.key, entry.value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(params (string key, object value)[] @params)
      => Make((IEnumerable<(string key, object value)>)@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(IEnumerable<(IModel.Builder.Param key, object value)> @params)
      => Make(@params.Select(entry => new KeyValuePair<IModel.Builder.Param, object>(entry.key, entry.value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(params (IModel.Builder.Param key, object value)[] @params)
      => Make((IEnumerable<(IModel.Builder.Param key, object value)>)@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(IEnumerable<KeyValuePair<IModel.Builder.Param, object>> @params)
      => Make(@params.Select(entry => new KeyValuePair<string,object>(entry.Key.Key, entry.Value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    protected internal TModelBase Make(params KeyValuePair<IModel.Builder.Param, object>[] @params)
      => Make((IEnumerable<KeyValuePair<IModel.Builder.Param, object>>)@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    protected internal TModelBase Make(params KeyValuePair<string, object>[] @params)
      => Make(@params.AsEnumerable());

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
    protected internal TDesiredModel Make<TDesiredModel>(params KeyValuePair<string, object>[] @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make(@params);

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
    protected internal TDesiredModel MakeAs<TDesiredModel>(out TDesiredModel model, params KeyValuePair<string, object>[] @params)
      where TDesiredModel : class, TModelBase
        => model = (TDesiredModel)Make(@params);

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

    /// <summary>
    /// Make a model that requires an object based builder:
    /// </summary>
    protected internal TDesiredModel MakeAs<TDesiredModel>(Action<IModel.Builder> configureBuilder, out TDesiredModel model)
     where TDesiredModel : TModelBase
        => model = (TDesiredModel)Make(configureBuilder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    protected internal TDesiredModel MakeAs<TDesiredModel>(IModel<TModelBase>.Builder builder, out TDesiredModel model)
      where TDesiredModel : TModelBase
        => model = (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    protected internal TDesiredModel MakeAs<TDesiredModel>(IBuilder<TModelBase> builder, out TDesiredModel model)
      where TDesiredModel : TModelBase
        => model = (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    protected internal TDesiredModel MakeAs<TDesiredModel>(Func<IModel<TModelBase>.Builder, IModel<TModelBase>.Builder> configureBuilder, out TDesiredModel model)
      where TDesiredModel : TModelBase
        => model = (TDesiredModel)BuildModel(configureBuilder((IModel<TModelBase>.Builder)MakeDefaultBuilder()));

    /// <summary>
    /// Make a model that requires a struct based builder"
    /// </summary>
    protected internal TDesiredModel MakeAs<TDesiredModel>(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder, out TDesiredModel model)
      where TDesiredModel : TModelBase
        => model = (TDesiredModel)BuildModel(configureBuilder(MakeDefaultBuilder()));

    #endregion

    #endregion

    #endregion
  }
}
