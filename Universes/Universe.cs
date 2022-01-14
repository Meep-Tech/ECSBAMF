﻿using Meep.Tech.Data.Configuration;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// A global collection of arechetypes.
  /// This is what the loader builds.
  /// </summary>
  public partial class Universe {

    /// <summary>
    /// All Universes
    /// </summary>
    public static IEnumerable<Universe> s
      => _all.Values;
    static Utility.OrderedDictionary<string, Universe> _all
      = new Utility.OrderedDictionary<string, Universe>();

    /// <summary>
    /// The unique key of this universe.
    /// </summary>
    public string Key {
      get;
    } = "";

    /// <summary>
    /// The loader used to build this universe
    /// </summary>
    public Loader Loader {
      get;
      internal set;
    }

    /// <summary>
    /// The model serializer instance for this universe
    /// </summary>
    public Model.Serializer ModelSerializer {
      get;
      internal set;
    }

    /// <summary>
    /// Archetypes data
    /// </summary>
    public ArchetypesData Archetypes {
      get;
    }

    /// <summary>
    /// Models data
    /// </summary>
    public ModelsData Models {
      get;
    }

    /// <summary>
    /// Components data
    /// </summary>
    public ComponentsData Components {
      get;
    }

    /// <summary>
    /// Enumerations Data
    /// </summary>
    public EnumerationData Enumerations {
      get;
    }

    /// <summary>
    /// Make a new universe of Archetypes
    /// </summary>
    public Universe(Loader loader, string nameKey = null) {
      Key = nameKey ?? Key;
      Loader = loader;
      Archetypes = new ArchetypesData(this);
      Models = new ModelsData(this);
      Components = new ComponentsData(this);
      Enumerations = new EnumerationData();

      // set this as the default universe if there isn't one yet
      Data.Archetypes.DefaultUniverse ??= this;
      _all.Add(Key, this);
    }

    /// <summary>
    /// Get a loaded universe by it's unique name key
    /// </summary>
    public static Universe Get(string nameKey)
      => _all[nameKey];
  }
}
