using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Meep.Tech.Data {

  /// <summary>
  /// A singleton data store and factory.
  /// </summary>
  public abstract partial class Archetype : IReadableComponentStorage, IEquatable<Archetype> {

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
    }

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
    /// Overrideable Model Constructor
    /// </summary>
    public virtual Func<IBuilder, IModel> ModelConstructor {
      get;
      internal set;
    } = null;

    /// <summary>
    /// The initial default components to add to this archetype on it's creation.
    /// </summary>
    protected virtual Dictionary<string, Archetype.IComponent> InitialComponents {
      get;
    } = new Dictionary<string, IComponent>();

    /// <summary>
    /// The default components to initialize on a model made by this Archetype.
    /// Usually you'll want to use an Archetype.ILinkedComponent but this is here too.
    /// </summary>
    protected virtual HashSet<System.Type> DefaultUnlinkedModelComponentTypes {
      get;
    } = new HashSet<Type>();

    /// <summary>
    /// If this is true, this Archetype can have it's component collection modified before load by mods and other libraries.
    /// This does not affect the ability to inherit and override InitialComponents for an archetype.
    /// </summary>
    public virtual bool AllowExternalComponentConfiguration
      => true;

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
      Id = id;
      Type = GetType();
    }

    /// <summary>
    /// helper for getting the builder constructor from the non-generic base class
    /// </summary>
    /// <returns></returns>
    internal protected abstract Func<Archetype, Dictionary<string, object>, IBuilder> GetGenericBuilderConstructor();

    #endregion

    #region Hash and Equality

    /// <summary>
    /// Used to convert an Archetype to a general string for storage
    /// </summary>
    public class ToKeyStringConverter : ValueConverter<Archetype, string> {
      public ToKeyStringConverter() : base(
        archetype => archetype.Id.Key,
        key => Archetypes.Id[key].Archetype
      ) {}
    }

    public override int GetHashCode() 
      => Id.GetHashCode();

    public override bool Equals(object obj) 
      => (obj as Archetype)?.Equals(this) ?? false;

    public override string ToString() {
      return $"+{Id}+";
    }

    public bool Equals(Archetype other) 
      => Id == other?.Id;

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
    public abstract IModel MakeDefault();
    
    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    public abstract IModel MakeDefaultWith(IBuilder builder);
    
    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    public abstract IModel MakeDefaultWith(Func<IBuilder, IBuilder> builderConfiguration);

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    public TDesiredModel Make<TDesiredModel>()
      where TDesiredModel : IModel
        => (TDesiredModel)MakeDefault();
    
    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    public TDesiredModel Make<TDesiredModel>(IBuilder builder)
      where TDesiredModel : IModel
        => (TDesiredModel)MakeDefaultWith(builder);

    /// <summary>
    /// Base make helper
    /// </summary>
    /// <returns></returns>
    public TDesiredModel Make<TDesiredModel>(Func<IBuilder, IBuilder> builderConfiguration)
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
    [IsModelComponentsProperty]
    Dictionary<string, Data.IComponent> _components {
      get;
    }



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
      where TComponent : Model.IComponent
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
    : Archetype
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
    /// The most basic model that this archetype family tree can produce
    /// </summary>
    public override System.Type ModelBaseType
      => typeof(TModelBase);

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public new Identity Id
      => base.Id as Identity;

    #endregion

    #region Archetype Initialization

    protected Archetype(Archetype.Identity id, ArchetypeCollection collection = null) 
      : base(id) {
      if(collection is null) {
        collection = (ArchetypeCollection)
          // if the base of this is registered somewhere, get the registered one by default
          (Archetypes._collectionsByRootArchetype.ContainsKey(id.Archetype.BaseType?.TryToGetAsArchetype()?.Id.Key)
            ? Archetypes.GetCollectionFor(this)
            // else this is the base and we need a new one
            : Archetypes._collectionsByRootArchetype[id.Key] = new ArchetypeCollection());
      }

      collection._registerArchetype(this);
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
      }
    }

    #endregion

    #region Model Construction 

    #region Model Constructor Settings

    /// <summary>
    /// Overrideable Model Constructor
    /// </summary>
    public new virtual Func<Model<TModelBase>.Builder, TModelBase> ModelConstructor {
      get {
        if(base.ModelConstructor == null) {
          base.ModelConstructor 
            = (builder) => GetDefaultCtorFor(typeof(TModelBase)).Invoke(((Model<TModelBase>.Builder)builder));
        }

        return (builder) => (TModelBase)base.ModelConstructor(builder);
      }
      protected internal set => base.ModelConstructor
       = builder => value.Invoke((Model<TModelBase>.Builder)builder);
    }

    /// <summary>
    /// Make an object ctor from a provided default ctor.
    /// Valid CTORS:
    ///  - public|private|protected Model(Model.Builder builder)
    ///  - public|private|protected Model()
    /// </summary>
    /// <param name="modelType"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static Func<Model<TModelBase>.Builder, TModelBase> GetDefaultCtorFor(Type modelType) {
      var ctor = modelType.GetConstructor(
        System.Reflection.BindingFlags.Public 
          | System.Reflection.BindingFlags.Instance
          | System.Reflection.BindingFlags.NonPublic,
        null,
        new[] { typeof(Model.Builder) },
        null
      );
      if(ctor is null) {
        ctor = modelType.GetConstructor(
          System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.NonPublic,
          null,
          new Type[0],
          null
        );
        if(!(ctor is null)) {
          //TODO: is there a faster way to cache this?
          return (builder)
            => (TModelBase)ctor.Invoke(null);
        }
      }
      if(ctor is null) {
        Func<Model<TModelBase>.Builder, TModelBase> activator 
          = _ => (TModelBase)System.Activator.CreateInstance(modelType);

        // this tests it to make sure it works:
        try {
          activator.Invoke(null);
          return activator;
        } catch {}
      }
      if(ctor is null) {
        throw new NotImplementedException($"No Ctor that takes a single argument of Model.Builder, or 0 arguments found for Model type: {modelType.FullName}. An activator could also not be built for the type.");
      }

      //TODO: is there a faster way to cache this?
      return (builder) 
        => (TModelBase)ctor.Invoke(new object[] {builder});
    }

    #endregion

    #region Build/Make

    #region Builder Setup

    /// <summary>
    /// An empty builder used to help build for this archetype:
    /// </summary>
    Model<TModelBase>.Builder _defaultEmptyBuilder
      = null;

    /// <summary>
    /// The default way a new builder is created.
    /// </summary>
    internal protected virtual Func<Archetype, Dictionary<string, object>, IBuilder<TModelBase>> BuilderConstructor {
      get => _defaultBuilderCtor ??= (archetype, @params) => new Model<TModelBase>.Builder(archetype, @params);
      set => _defaultBuilderCtor = value;
    } internal Func<Archetype, Dictionary<string, object>, IBuilder<TModelBase>> _defaultBuilderCtor;

    /// <summary>
    /// helper for getting the builder constructor from the non-generic base class
    /// TODO: I can probably cache this at least.
    /// </summary>
    protected internal sealed override Func<Archetype, Dictionary<string, object>, IBuilder> GetGenericBuilderConstructor()
      => _defaultBuilderCtor;

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    public virtual Model<TModelBase>.Builder MakeDefaultBuilder()
      => (Model<TModelBase>.Builder)BuilderConstructor(this, null);

    /// <summary>
    /// Gets an immutable empty builder for this type to use when null was passed in:
    /// </summary>
    protected virtual Model<TModelBase>.Builder MakeDefaultEmptyBuilder()
      => (Model<TModelBase>.Builder)MakeDefaultBuilder().AsImmutable();

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
    public virtual TModelBase Make(IEnumerable<KeyValuePair<string, object>> @params)
      => BuildModel(MakeDefaultBuilder().Merge(@params.ToDictionary(
        param => param.Key,
        param => param.Value
      )) as Model<TModelBase>.Builder);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public TModelBase Make(IEnumerable<(string key, object value)> @params)
      => Make(@params.Select(entry => new KeyValuePair<string,object>(entry.key, entry.value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public TModelBase Make(params (string key, object value)[] @params)
      => Make((IEnumerable<(string key, object value)>)@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public TModelBase Make(IEnumerable<(Model.Builder.Param key, object value)> @params)
      => Make(@params.Select(entry => new KeyValuePair<Model.Builder.Param, object>(entry.key, entry.value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public TModelBase Make(params (Model.Builder.Param key, object value)[] @params)
      => Make((IEnumerable<(Model.Builder.Param key, object value)>)@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public TModelBase Make(IEnumerable<KeyValuePair<Model.Builder.Param, object>> @params)
      => Make(@params.Select(entry => new KeyValuePair<string,object>(entry.Key.Key, entry.Value)));

    /// <summary>
    /// Helper for potentially making an item without initializing a Builder object.
    /// </summary>
    public TModelBase Make(params KeyValuePair<Model.Builder.Param, object>[] @params)
      => Make((IEnumerable<KeyValuePair<Model.Builder.Param, object>>)@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    public TModelBase Make(params KeyValuePair<string, object>[] @params)
      => Make(@params.AsEnumerable());

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    public TDesiredModel Make<TDesiredModel>(IEnumerable<KeyValuePair<string, object>> @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make(@params);

    /// <summary>
    /// Helper for potentially making an item without initializing a dictionary object
    /// </summary>
    /// <returns></returns>
    public TDesiredModel Make<TDesiredModel>(params KeyValuePair<string, object>[] @params)
      where TDesiredModel : TModelBase
        => (TDesiredModel)Make(@params);

    #endregion

    #region Builder Based

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    /// <returns></returns>
    public sealed override IModel MakeDefault()
      => BuildModel(null);

    /// <summary>
    /// Make a default model from this Archetype with the builder
    /// </summary>
    /// <returns></returns>
    public sealed override IModel MakeDefaultWith(Func<IBuilder, IBuilder> builderConfiguration)
      => Make(builderConfiguration);
    /// <summary>
    /// Make a default model from this Archetype with the builder
    /// </summary>
    /// <returns></returns>
    public sealed override IModel MakeDefaultWith(IBuilder builder)
      => Make(builder as IBuilder<TModelBase>);

    /// <summary>
    /// Make a default model from this Archetype
    /// </summary>
    /// <returns></returns>
    public TModelBase Make()
      => BuildModel(null);

    public TModelBase Make(Func<Model<TModelBase>.Builder, Model<TModelBase>.Builder> configureBuilder)
      => BuildModel(configureBuilder(MakeDefaultBuilder()));

    public TModelBase Make(Model<TModelBase>.Builder builder)
      => BuildModel(builder);

    public TModelBase Make(IBuilder<TModelBase> builder)
      => BuildModel(builder);

    /// <summary>
    /// Make a default model from this Archetype of the desired sub-type
    /// </summary>
    public new TDesiredModel Make<TDesiredModel>()
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(null);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public TDesiredModel Make<TDesiredModel>(Model<TModelBase>.Builder builder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model from this archetype using a fully qualified builder.
    /// </summary>
    public TDesiredModel Make<TDesiredModel>(IBuilder<TModelBase> builder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(builder);

    /// <summary>
    /// Make a model from this archetype by passing down and updating a default builder.
    /// </summary>
    public TDesiredModel Make<TDesiredModel>(Func<Model<TModelBase>.Builder, Model<TModelBase>.Builder> configureBuilder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(configureBuilder(MakeDefaultBuilder()));

    /// <summary>
    /// Make a model that requires a struct based builder:
    /// </summary>
    public TModelBase Make(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      => BuildModel(configureBuilder(MakeDefaultBuilder()));

    /// <summary>
    /// Make a model that requires a struct based builder:
    /// </summary>
    public TModelBase Make(Func<IBuilder, IBuilder> configureBuilder)
      => Make(builder => (configureBuilder(MakeDefaultBuilder()) as IBuilder<TModelBase>));

    /// <summary>
    /// Make a model that requires an object based builder:
    /// </summary>
    public TDesiredModel Make<TDesiredModel>(Action<Model.Builder> configureBuilder)
     where TDesiredModel: TModelBase
        => (TDesiredModel)Make(configureBuilder);

    /// <summary>
    /// Make a model that requires an object based builder:
    /// </summary>
    public TModelBase Make(Action<Model.Builder> configureBuilder)
      => Make(builder => {
        configureBuilder(builder);

        return builder;
      });

    /// <summary>
    /// Make a model that requires a struct based builder"
    /// </summary>
    public TDesiredModel Make<TDesiredModel>(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(configureBuilder(MakeDefaultBuilder()));

    /// <summary>
    /// Build the model with the builder.
    /// </summary>
    protected internal virtual  TModelBase BuildModel(IBuilder<TModelBase> builder = null) {
      var builderToUse = builder;
      if(builder is null) {
        builderToUse = _defaultEmptyBuilder ??= MakeDefaultEmptyBuilder();
      }

      var model = builderToUse.Build();
      if(model is IReadableComponentStorage storage) {
        _addModelComponentsToModel(storage, builder);
      }

      return model;
    }

    TModelBase _addModelComponentsToModel(IReadableComponentStorage model, IBuilder<TModelBase> builder = null) {
      foreach(Type componentType in DefaultUnlinkedModelComponentTypes) {
        // Make a builder to match this component with the params from the parent:
        IBuilder componentBuilder 
          = Model.Builder.MakeNewBuilderAndCopyParams(builder, componentType);

        // build the component:
        Model.IComponent component = (Data.Components.GetBuilderFactoryFor(componentType) as Archetype)
          .MakeDefaultWith(componentBuilder) as Model.IComponent;

        model.AddComponent(component);
      }

      return (TModelBase)model;
    }

    #endregion

    #endregion

    #endregion
  }
}
