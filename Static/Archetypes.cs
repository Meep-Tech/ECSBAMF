using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  /// <summary>
  /// TODO: Sets of Archetypes could be stored in a Universe accessable from each Identity within that universe via Identity.Universe or something.
  /// </summary>
  public static class Archetypes {

    #region Data Access

    /// <summary>
    /// All archetypes:
    /// </summary>
    public static Archetype.Collection All {
      get;
    } = new Archetype.Collection();

    /// <summary>
    /// All registered Archetype Identities
    /// </summary>
    public static IEnumerable<Archetype.Identity> Ids
      => _ids.Values;
    static Dictionary<string, Archetype.Identity> _ids
      = new Dictionary<string, Archetype.Identity>();

    /// <summary>
    /// Ids, indexed by external id value
    /// </summary>
    public static IReadOnlyDictionary<string , Archetype.Identity> Id
      => _ids;

    /// <summary>
    /// All archetypes:
    /// </summary>
    public static IEnumerable<Archetype.Collection> Collections {
      get => _collectionsByRootArchetype.Values.Distinct();
    } internal static readonly Dictionary<string, Archetype.Collection> _collectionsByRootArchetype
      = new Dictionary<string, Archetype.Collection>();

    /// <summary>
    /// Get a collection registered to an archetype root:
    /// </summary>
    public static Archetype.Collection GetCollectionFor(Archetype root)
      => _collectionsByRootArchetype.TryGetValue(root.Id.Key, out Archetype.Collection collection)
        ? collection
        // recurse until it's found. This should throw a null exception eventually if one isn't found.
        : GetCollectionFor(root.Type.BaseType.TryToGetAsArchetype());

    #endregion

    #region Conversions

    /// <summary>
    /// Get a system type as an archetype.
    /// </summary>
    public static Archetype AsArchetype(this System.Type type)
      => All.Get(type);

    /// <summary>
    /// Get a system type as an archetype.
    /// </summary>
    public static Archetype TryToGetAsArchetype(this System.Type type)
      => All.TryToGet(type);

    /// <summary>
    /// Get a system type as an archetype.
    /// </summary>
    public static TArchetype AsArchetype<TArchetype>(this System.Type type)
      where TArchetype : Archetype
        => All.Get<TArchetype>();

    #endregion
  }

  /// <summary>
  /// Static data for archetypes, by archetype class.
  /// </summary>
  public static class Archetypes<TArchetype> 
    where TArchetype : Archetype {

    /// <summary>
    /// The instance of this archetype type
    /// </summary>
    public static TArchetype Instance
      => (TArchetype)typeof(TArchetype).AsArchetype();

    /// <summary>
    /// The instance of this archetype type
    /// </summary>
    public static TArchetype Archetype
      => Instance;

    /// <summary>
    /// The instance of this archetype type
    /// </summary>
    public static TArchetype _
      => Instance;

    /// <summary>
    /// The instance of this archetype type
    /// This is because ._. looks sad sometimes, so you can use .u. to cheer them up.
    /// </summary>
    public static TArchetype u
      => Instance;

    /// <summary>
    /// The instance of this archetype type
    /// uwu
    /// </summary>
    public static TArchetype w
      => Instance;

    /// <summary>
    /// Helper to get the collection for this archetype:
    /// </summary>
    public static Archetype.Collection Collection
      => Archetypes.GetCollectionFor(typeof(TArchetype).AsArchetype());
  }
}
