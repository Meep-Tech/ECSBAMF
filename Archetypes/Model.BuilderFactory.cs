using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  public partial interface IModel {

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// This is the base non-generic utility class
    /// </summary>
    public interface IBuilderFactory {

      /// <summary>
      /// Overrideable Model Constructor
      /// </summary>
      Func<IBuilder, IModel> ModelConstructor {
        get;
        internal set;
      }
    }
  }

  public partial class Model<TModelBase> where TModelBase : IModel<TModelBase> {

    /// <summary>
    /// The factory that was used to make this object
    /// </summary>
    public IModel<TModelBase>.BuilderFactory DefaultFactory
      => Models<TModelBase>.BuilderFactory;
  }

  public partial interface IModel<TModelBase> where TModelBase : IModel<TModelBase> {

    /// <summary>
    /// The default builder for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// They can be overriden.
    /// </summary>
    public class BuilderFactory 
      : Archetype<TModelBase, BuilderFactory>,
      IBuilderFactory 
    {

      Func<IBuilder, IModel> IBuilderFactory.ModelConstructor {
        get => builder => base.ModelConstructor((Builder)builder);
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
      public new virtual Func<Archetype, Dictionary<string, object>, IBuilder<TModelBase>> BuilderConstructor {
        get => _defaultBuilderCtor ??= (archetype, @params) => base.BuilderConstructor(archetype, @params) as Builder;
        set => _defaultBuilderCtor = value;
      }

      internal protected BuilderFactory(Archetype.Identity id)
        : base(
            id,
            (ArchetypeCollection)(id.Universe.Models._factoriesByModelBases
              .TryGetValue(typeof(TModelBase), out var collection)
                ? collection 
                : id.Universe.Models._factoriesByModelBases[typeof(TModelBase)] 
                  = new ArchetypeCollection(id.Universe))
        )
      {
        Id.Universe.Models._factories.Add(this);
      }
    }

  }
}