using System;
using System.Collections.Generic;
using static Meep.Tech.Data.Configuration.Loader.Settings;

namespace Meep.Tech.Data {

  public partial interface IModel {

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// This is the base non-generic utility class
    /// </summary>
    public interface IFactory : Data.IFactory {}
  }

  public partial interface IModel<TModelBase> where TModelBase : IModel<TModelBase> {

    /// <summary>
    /// The default builder for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// They can be overriden.
    /// </summary>
    [Configuration.Loader.Settings.DoNotBuildInInitialLoad]
    public new class Factory
      : Factory<Factory> {

      public Factory(
        Identity id,
        Universe universe,
        HashSet<Data.Archetype.IComponent> archetypeComponents,
        IEnumerable<Func<IBuilder, IModel.IComponent>> modelComponentCtors 
      )  : base(id, universe, archetypeComponents, modelComponentCtors) { }

      public Factory(
        Identity id,
        Universe universe = null
      )  : base(id, universe) { }
    }

    /// <summary>
    /// The base of all BuilderFactories.
    /// Custom factories aren't built initially, you should maintain the singleton pattern yourself by setting it
    /// in the static constructor, or the Setup(Universe) override
    /// </summary>
    [DoNotBuildThisOrChildrenInInitialLoad]
    public abstract class Factory<TBuilderFactoryBase>
      : Archetype<TModelBase, TBuilderFactoryBase>,
        Archetype<TModelBase, TBuilderFactoryBase>.IExposeDefaultModelBuilderMakeMethods.Fully,
        IModel.IFactory 
      where TBuilderFactoryBase : Factory<TBuilderFactoryBase>
    {

      /// <summary>
      /// Used for Buidler Factories to easily change the base type
      /// </summary>
      public new Type ModelBaseType {
        get => base.ModelBaseType;
        init => base.ModelBaseType = value;
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      Func<Data.IBuilder, IModel> Data.IFactory._modelConstructor {
        get => base.ModelConstructor is null 
          ? null 
          : builder => base.ModelConstructor((IBuilder<TModelBase>)builder);
        set => base.ModelConstructor = 
          builder => (TModelBase)value(builder);
      }

      /// <summary>
      /// The static instance of this type of builder factory.
      /// </summary>
      public static Factory DefaultInstance
        => Archetypes.All.Get<Factory>();

      /// <summary>
      /// The static instance of this type of builder factory.
      /// </summary>
      public static Factory InstanceFor(Universe universe)
        => universe.Archetypes.All.Get<Factory>();

      /// <summary>
      /// The default way a new builder is created.
      /// This can be used to set this for a Model<> without archetypes.
      /// </summary>
      public new virtual Func<Data.Archetype, Dictionary<string, object>, Universe, IBuilder<TModelBase>> BuilderConstructor {
        get => _defaultBuilderCtor ??= (archetype, @params, universe) 
          => base.BuilderConstructor(archetype, @params, universe);
        init => _defaultBuilderCtor = value;
      }

      internal protected Factory(
        Data.Archetype.Identity id,
        Universe universe = null,
        HashSet<Data.Archetype.IComponent> archetypeComponents = null,
        IEnumerable<Func<IBuilder, IModel.IComponent>> modelComponentCtors = null
      ) : base(
            id,
            (Collection)((universe ?? Models.DefaultUniverse).Models._factoriesByModelBases
              .TryGetValue(typeof(TModelBase), out var collection)
                ? collection 
                : (universe ?? Models.DefaultUniverse).Models._factoriesByModelBases[typeof(TModelBase)] 
                  = new Collection((universe ?? Models.DefaultUniverse)))
        )
      {
        Id.Universe.Models._factories.Add(this);
      }
    }
  }
}