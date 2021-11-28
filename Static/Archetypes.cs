using System.Collections.Generic;

namespace Meep.Tech.Data {
  public static class Archetypes {

    /// <summary>
    /// All archetypes:
    /// </summary>
    public static Archetype.Collection All {
      get;
    } = new Archetype.Collection();

    /// <summary>
    /// All archetypes:
    /// </summary>
    public static IEnumerable<Archetype.Collection> Collections {
      get => _collectionsByRootArchetype.Values;
    } internal static readonly Dictionary<string, Archetype.Collection> _collectionsByRootArchetype
      = new Dictionary<string, Archetype.Collection>();

    /// <summary>
    /// Get a system type as an archetype.
    /// </summary>
    public static Archetype AsArchetype(this System.Type type)
      => All.Get(type);

    /// <summary>
    /// Get a system type as an archetype.
    /// </summary>
    public static TArchetype AsArchetype<TArchetype>(this System.Type type)
      where TArchetype : Archetype
        => All.Get<TArchetype>();
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
  }
}
