using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public abstract partial class Archetype : IEquatable<Archetype> {

    /// <summary>
    /// A Collection of Archetypes.
    /// All archetypes need to be in a collection of some kind.
    /// This is the base non-generic utility class for collections.
    /// </summary>
    public class Collection :
        IEnumerable<Archetype> {

      /// <summary>
      /// Generic collections have no root archetype.
      /// </summary>
      public virtual Archetype RootArchetype
        => null;

      /// <summary>
      /// All archetypes registered to this collection
      /// </summary>
      public IEnumerable<Archetype> All
        => _byId.Values;

      /// <summary>
      /// All archetypes registered to this collection by their Identity.
      /// </summary>
      public IReadOnlyDictionary<Archetype.Identity, Archetype> ById {
        get => _byId;
      }
      internal readonly Dictionary<Archetype.Identity, Archetype> _byId
        = new Dictionary<Archetype.Identity, Archetype>();

      /// <summary>
      /// All archetypes:
      /// </summary>
      public IReadOnlyDictionary<string, Archetype> ByFullTypeName {
        get => _byType;
      } internal readonly Dictionary<string, Archetype> _byType
      = new Dictionary<string, Archetype>();

      /// <summary>
      /// All archetypes, indexed by their base model types.
      /// </summary>
      internal static readonly Dictionary<string, Archetype> _byModelBaseType
      = new Dictionary<string, Archetype>();

      #region Initialization

      internal Collection() {
        Archetypes._collectionsByRootArchetype.Add(RootArchetype?.Id.Key ?? "_all", this);
      }

      internal void _registerArchetype(Archetype @new) {
        // Register to this:
        Add(@new);

        // Add to All:
        Archetypes.All.Add(@new);
      }

      /// <summary>
      /// Add an archetype to this collection.
      /// This does NOT register the archetype, this can only be used to add a
      /// previously registered archetype to another collection.
      /// </summary>
      public void Add(Archetype archetype) {
        _byId.Add(archetype.Id, archetype);
        _byType.Add(archetype.GetType().FullName, archetype);
        _byModelBaseType.Add(archetype.ModelBaseType.FullName, archetype);
      }

      #endregion

      #region Accessors

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// </summary>
      public TArchetype Get<TArchetype>()
        where TArchetype : Archetype
          => Archetypes<TArchetype>.Instance;

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// </summary>
      public Archetype Get(System.Type type)
          => _byType[type.FullName];

      #endregion

      #region Enumeration

      public IEnumerator<Archetype> GetEnumerator()
        => All.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

      #endregion
    }

    /// <summary>
    /// A Collection of Archetypes.
    /// - Can be used to make a more specific child collection.
    /// </summary>
    public class Collection<TModelBase, TArchetypeBase>
      : Archetype<TModelBase, TArchetypeBase>.Collection
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {
      public Collection() : base() {}
    }
  }

  public abstract partial class Archetype<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// A Collection of Archetypes.
    /// All archetypes need to be in a collection of some kind.
    /// </summary>
    public new class Collection 
      : Archetype.Collection,
        IEnumerable<TArchetypeBase> 
    {

      /// <summary>
      /// Generic collections have no root archetype.
      /// </summary>
      public override Archetype RootArchetype
        => typeof(TArchetypeBase).AsArchetype();

      /// <summary>
      /// All archetypes registered to this collection
      /// </summary>
      public new IEnumerable<TArchetypeBase> All
        => _byId.Values.Cast<TArchetypeBase>();

      /// <summary>
      /// All archetypes registered to this collection by their Identity.
      /// </summary>
      public new IReadOnlyDictionary<Identity, TArchetypeBase> ById {
        get => _compiledById ??= _byId.ToDictionary(
          archetype => archetype.Key as Identity,
          archetype => archetype.Value as TArchetypeBase
        );
      } Dictionary<Identity, TArchetypeBase> _compiledById;

      public Collection() : base() {}

      #region Accessors

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// </summary>
      public new TArchetype Get<TArchetype>()
        where TArchetype : TArchetypeBase
          => Archetypes<TArchetype>.Instance;

      #endregion

      #region Enumeration

      public new IEnumerator<TArchetypeBase> GetEnumerator()
        => All.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

      #endregion
    }
  }
}
