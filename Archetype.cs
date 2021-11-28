using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Meep.Tech.Data {

  /// <summary>
  /// A singleton data store and factory.
  /// </summary>
  public abstract partial class Archetype : IEquatable<Archetype>, IComponentStorage {

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public Identity Id {
      get;
    }

    /// <summary>
    /// The Base Archetype this Archetype derives from.
    /// </summary>
    public abstract Archetype Base {
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
    /// Overrideable Model Constructor
    /// </summary>
    public virtual Func<IBuilder, IModel> ModelConstructor {
      get;
      internal set;
    } = null;

    /// <summary>
    /// If this is an archetype that inherits from Archetype<> directly.
    /// </summary>
    public bool IsBase
      => Base?.Equals(null) ?? true;

    /// <summary>
    /// Make a new archetype
    /// </summary>
    protected Archetype(Identity id) {
      Id = id;
      Type = GetType();
    }

    #region Hash and Equality

    public override int GetHashCode() {
      return Id.GetHashCode();
    }

    public override bool Equals(object obj) {
      return obj?.GetType() == GetType();
    }

    public override string ToString() {
      return $"+{Id}+";
    }

    public bool Equals(Archetype other) 
      => Id == other?.Id;

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

    /// <summary>
    /// The base archetype that all of the ones like it are based on.
    /// </summary>
    public override Archetype Base
      => typeof(TArchetypeBase).AsArchetype();

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

    protected Archetype(Archetype.Identity id, Collection collection) 
      : base(id) {
      collection._registerArchetype(this);
    }

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
    Model<TModelBase>.Builder _defaultBuilder
      = null;

    /// <summary>
    /// The default way a new builder is created.
    /// </summary>
    public Func<Archetype, IBuilder> NewBuilderConstructor {
      get => _defaultBuilderCtor ??= archetype => new Model<TModelBase>.Builder(archetype);
      internal set => _defaultBuilderCtor = value;
    } Func<Archetype, IBuilder> _defaultBuilderCtor;

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    protected virtual Model<TModelBase>.Builder MakeDefaultBuilder()
      => (Model<TModelBase>.Builder)NewBuilderConstructor(this);

    /// <summary>
    /// Gets an immutable empty builder for this type to use when null was passed in:
    /// </summary>
    protected virtual Model<TModelBase>.Builder MakeDefaultEmptyBuilder()
      => (Model<TModelBase>.Builder)MakeDefaultBuilder().asImmutable();

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
      => Make(@params);

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
    public TDesiredModel Make<TDesiredModel>()
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
    /// Make a model that requires a struct based builder"
    /// </summary>
    public TDesiredModel Make<TDesiredModel>(Func<IBuilder<TModelBase>, IBuilder<TModelBase>> configureBuilder)
      where TDesiredModel : TModelBase
        => (TDesiredModel)BuildModel(configureBuilder(MakeDefaultBuilder()));

    /// <summary>
    /// Build the model with the builder.
    /// </summary>
    protected internal TModelBase BuildModel(IBuilder<TModelBase> builder = null) {
      var builderToUse = builder;
      if(builder is null) {
        builderToUse = _defaultBuilder ??= MakeDefaultEmptyBuilder();
      }

      return builderToUse.build();
    }

    #endregion

    #endregion

    #endregion
  }
}
