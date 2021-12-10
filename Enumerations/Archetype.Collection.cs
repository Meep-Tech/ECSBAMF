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
        get => _byId.ToDictionary(
          entry => Archetypes.Id[entry.Key],
          entry => entry.Value
        );
      } internal readonly Dictionary<string, Archetype> _byId
        = new Dictionary<string, Archetype>();

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
      internal readonly Dictionary<string, Archetype> _byModelBaseType
        = new Dictionary<string, Archetype>();

      #region Initialization

      internal Collection() {
        Archetypes._collectionsByRootArchetype.Add(RootArchetype?.Id.Key ?? "_all", this);
      }

      internal void _registerArchetype(Archetype @new) {
        // Register to it's id
        @new.Id.Archetype = @new;

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
        _byId.Add(archetype.Id.Key, archetype);
        _byType.Add(archetype.GetType().FullName, archetype);
        _byModelBaseType.Add(archetype.ModelBaseType.FullName, archetype);
      }

      #endregion

      #region Accessors

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public TArchetype Get<TArchetype>()
        where TArchetype : Archetype
          => Archetypes<TArchetype>.Instance;

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public Archetype Get(System.Type type)
          => _byType[type.FullName];

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public Archetype TryToGet(System.Type type)
          => _byType.TryGetValue(type.FullName, out var found)
            ? found
            : null;

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// </summary>
      public bool TryToGet(System.Type type, out Archetype found)
          => _byType.TryGetValue(type.FullName, out found);

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public Archetype Get(Identity id)
          => _byId[id.Key];

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public Archetype Get(string externalId)
          => _byId[externalId];

      /// <summary>
      /// Try to get an archetype from this collection by it's externalId.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public Archetype TryToGet(string externalId)
          => _byId.TryGetValue(externalId, out var found)
            ? found
            : null;

      /// <summary>
      /// Try to get an archetype from this collection by it's externalId.
      /// </summary>
      public bool TryToGet(string externalId, out Archetype found)
          => _byId.TryGetValue(externalId, out found);

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
    /// - Can be used to make a more specific child collection than Archetype<,>.ArchetypeCollection.
    /// </summary>
    public class Collection<TModelBase, TArchetypeBase>
      : Archetype<TModelBase, TArchetypeBase>.ArchetypeCollection
      where TModelBase : IModel<TModelBase>
      where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {
      public Collection() : base() {}
    }
  }

  public abstract partial class Archetype<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// Quick link to the collection
    /// </summary>
    public new static ArchetypeCollection Collection
      => (ArchetypeCollection)
        Archetypes.GetCollectionFor(typeof(TArchetypeBase).AsArchetype());

    /// <summary>
    /// A Collection of Archetypes.
    /// This is just an Archetype.Collection<,> that is pre-built for the containing Archetype<,> type.
    /// </summary>
    public class ArchetypeCollection 
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

      public ArchetypeCollection() : base() {}

      #region Accessors

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public new TArchetype Get<TArchetype>()
        where TArchetype : TArchetypeBase
          => base.Get<TArchetype>();

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public new TArchetypeBase Get(System.Type type)
          => (TArchetypeBase)base.Get(type);

      /// <summary>
      /// Get an archetype from this collection by it's type.
      /// </summary>
      public TArchetype Get<TArchetype>(System.Type type)
        where TArchetype : TArchetypeBase
          => (TArchetype)Get(type);

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public new TArchetypeBase TryToGet(System.Type type)
          => (TArchetypeBase)base.TryToGet(type);

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public TArchetype TryToGet<TArchetype>(System.Type type)
        where TArchetype : TArchetypeBase
          => (TArchetype)TryToGet(type);

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public bool TryToGet(System.Type type, out TArchetypeBase found) {
        if(base.TryToGet(type, out var foundType)) {
        found = foundType as TArchetypeBase;
          if (found != null) return true;
        }

        found = null;
        return false;
      }

      /// <summary>
      /// Try to get an archetype from this collection by it's type.
      /// Returns null on failure instead of throwing.
      /// </summary>
      public bool TryToGet<TArchetype>(System.Type type, out TArchetype found)
        where TArchetype : TArchetypeBase {
        if(TryToGet(type, out var foundArchetype)) {
          found = foundArchetype as TArchetype;
          if(found != null)
            return true;
        }

        found = null;
        return false;
      }

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public TArchetypeBase Get(Identity id)
          => (TArchetypeBase)base.Get(id);

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public TArchetype Get<TArchetype>(Identity id)
        where TArchetype : TArchetypeBase
          => (TArchetype)Get(id);

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public TArchetype Get<TArchetype>(string externalId)
        where TArchetype : TArchetypeBase
          => (TArchetype)Get(externalId);

      /// <summary>
      /// Get an archetype from it's Id.
      /// </summary>
      public new TArchetypeBase Get(string externalId)
          => (TArchetypeBase)base.Get(externalId);

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
