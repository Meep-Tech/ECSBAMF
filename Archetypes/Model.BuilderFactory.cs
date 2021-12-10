using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  public partial class Model {

    /// <summary>
    /// The collection of all base model BuilderFactories.
    /// </summary>
    internal static Archetype.Collection _factories
      = new Archetype.Collection();

    /// <summary>
    /// The collection of all base model BuilderFactories.
    /// </summary>
    internal static Dictionary<Type, Model.IBuilderFactory> _factoriesByModelBase
      = new Dictionary<Type, Model.IBuilderFactory>();

    /// <summary>
    /// The default factory for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// This is the base non-generic utility class
    /// </summary>
    public interface IBuilderFactory {}
  }

  public partial class Model<TModelBase> where TModelBase : IModel<TModelBase> {

    /// <summary>
    /// The builder collection for these types:
    /// </summary>
    internal new static Archetype<TModelBase, BuilderFactory>.ArchetypeCollection _factories
      = new Archetype<TModelBase, BuilderFactory>.ArchetypeCollection();

    /// <summary>
    /// The factory that was used to make this object
    /// </summary>
    public BuilderFactory Factory 
      => Models<TModelBase>.BuilderFactory;

    /// <summary>
    /// The default builder for Models without Archetypes.
    /// One of these is instantiated for each Model<> class and IComponent<> class by default.
    /// They can be overriden.
    /// </summary>
    public class BuilderFactory 
      : Archetype<TModelBase, BuilderFactory>,
      IBuilderFactory 
    {

      /// <summary>
      /// The static instance of this type of builder factory.
      /// </summary>
      public static BuilderFactory Instance
        => Model._factories.Get<BuilderFactory>();

      /// <summary>
      /// The default way a new builder is created.
      /// This can be used to set this for a Model<> without archetypes.
      /// </summary>
      public new virtual Func<Archetype, Dictionary<string, object>, IBuilder<TModelBase>> BuilderConstructor {
        get => _defaultBuilderCtor ??= (archetype, @params) => base.BuilderConstructor(archetype, @params) as Builder;
        set => _defaultBuilderCtor = value;
      }

      internal protected BuilderFactory(Archetype.Identity id)
        : base(id, Model<TModelBase>._factories) {
        Model._factories.Add(this);
      }
    }

  }
}