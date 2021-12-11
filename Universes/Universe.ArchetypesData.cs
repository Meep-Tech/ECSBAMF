using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Universe {
    /// <summary>
    /// Used to hold the data for all archetypes
    /// </summary>
    public class ArchetypesData {

      /// <summary>
      /// All archetypes:
      /// </summary>
      public Archetype.Collection All {
        get;
      }

      /// <summary>
      /// All registered Archetype Identities
      /// </summary>
      public IEnumerable<Archetype.Identity> Ids
        => _ids.Values;
      Dictionary<string, Archetype.Identity> _ids
        = new Dictionary<string, Archetype.Identity>();

      /// <summary>
      /// Ids, indexed by external id value
      /// </summary>
      public IReadOnlyDictionary<string, Archetype.Identity> Id
        => _ids;

      /// <summary>
      /// All archetypes:
      /// </summary>
      public IEnumerable<Archetype.Collection> Collections {
        get => _collectionsByRootArchetype.Values.Distinct();
      }
      internal readonly Dictionary<string, Archetype.Collection> _collectionsByRootArchetype
      = new Dictionary<string, Archetype.Collection>();

      internal ArchetypesData(Universe universe) {
        All = new Archetype.Collection(universe);
      }

      /// <summary>
      /// Get a collection registered to an archetype root:
      /// </summary>
      public Archetype.Collection GetCollectionFor(Archetype root)
        => _collectionsByRootArchetype.TryGetValue(root.Id.Key, out Archetype.Collection collection)
          ? collection
          // recurse until it's found. This should throw a null exception eventually if one isn't found.
          : GetCollectionFor(root.Type.BaseType.TryToGetAsArchetype());

    }
  }
}
