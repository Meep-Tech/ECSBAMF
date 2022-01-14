using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public abstract partial class Archetype<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// A Collection of Archetypes.
    /// This is just an Archetype.Collection[&#44;] that is pre-built for the containing Archetype[&#44;] type.
    /// </summary>
    public class ArchetypeCollection 
      : Archetype.Collection,
        IEnumerable<TArchetypeBase> 
    {

      /// <summary>
      /// The root archetype. This may be null if the root archetype type is abstract.
      /// </summary>
      public override Archetype RootArchetype
        => typeof(TArchetypeBase).TryToGetAsArchetype();

      /// <summary>
      ///  The archetype type of the root archetype of this collection (if it's not abstract).
      /// </summary>
      public override System.Type RootArchetypeType
        => typeof(TArchetypeBase);

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
          archetype => (Identity)Universe.Archetypes.Id[archetype.Key],
          archetype => (TArchetypeBase)archetype.Value
        );
      } Dictionary<Identity, TArchetypeBase> _compiledById;

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public ArchetypeCollection(Universe universe = null) 
        : base(universe ?? Archetypes.DefaultUniverse) {}

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

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public new IEnumerator<TArchetypeBase> GetEnumerator()
        => All.GetEnumerator();

      IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

      #endregion
    }
  }
}
