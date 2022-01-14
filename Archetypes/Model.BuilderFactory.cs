using Newtonsoft.Json;
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
    public interface IBuilderFactory : IFactory {}
  }

  public partial class Model<TModelBase> where TModelBase : Model<TModelBase> {

    /// <summary>
    /// The factory that was used to make this object
    /// </summary>
    [IsArchetypeProperty]
    public IModel.IBuilderFactory Factory {
      get;
      private set;
    }
  }

  public partial interface IModel<TModelBase> where TModelBase : IModel<TModelBase> {

    /// <summary>
    /// The default builder for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// They can be overriden.
    /// </summary>
    public class BuilderFactory
      : BuilderFactory<BuilderFactory> {

      public BuilderFactory(
        Archetype.Identity id,
        Universe universe,
        HashSet<Archetype.IComponent> archetypeComponents,
        IEnumerable<Func<IBuilder, IModel.IComponent>> modelComponentCtors 
      )  : base(id, universe, archetypeComponents, modelComponentCtors) { }
      public BuilderFactory(
        Archetype.Identity id,
        Universe universe = null
      )  : base(id, universe) { }
    }

    /// <summary>
    /// The base of all BuilderFactories.
    /// Custom factories aren't built initially, you should maintain the singleton pattern yourself by setting it
    /// in the static constructor, or the Setup(Universe) override
    /// </summary>
    [DoNotBuildThisOrChildrenInInitialLoad]
    public abstract class BuilderFactory<TBuilderFactoryBase>
      : Archetype<TModelBase, TBuilderFactoryBase>,
      IBuilderFactory 
      where TBuilderFactoryBase : BuilderFactory<TBuilderFactoryBase>
      {

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      Func<IBuilder, IModel> IFactory.ModelConstructor {
        get => builder => base.ModelConstructor((IBuilder<TModelBase>)builder);
        set => base.ModelConstructor = 
          builder => (TModelBase)value(builder);
      }

      /// <summary>
      /// The static instance of this type of builder factory.
      /// </summary>
      public static BuilderFactory DefaultInstance
        => Archetypes.All.Get<BuilderFactory>();

      /// <summary>
      /// The static instance of this type of builder factory.
      /// </summary>
      public static BuilderFactory InstanceFor(Universe universe)
        => universe.Archetypes.All.Get<BuilderFactory>();

      /// <summary>
      /// The default way a new builder is created.
      /// This can be used to set this for a Model<> without archetypes.
      /// </summary>
      public new virtual Func<Archetype, Dictionary<string, object>, Universe, IBuilder<TModelBase>> BuilderConstructor {
        get => _defaultBuilderCtor ??= (archetype, @params, universe) => base.BuilderConstructor(archetype, @params, universe) as Builder;
        set => _defaultBuilderCtor = value;
      }

      internal protected BuilderFactory(
        Archetype.Identity id,
        Universe universe = null,
        HashSet<Archetype.IComponent> archetypeComponents = null,
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