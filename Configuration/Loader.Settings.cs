using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Meep.Tech.Data.Configuration {

  public partial class Loader {

    /// <summary>
    /// Settings for the loader
    /// </summary>
    public partial class Settings {

      /// <summary>
      /// Assemblies that should be included in the loading that are built in.
      /// This helps prevent assemblies from not being loaded yet on initial searches
      /// </summary>
      public List<Assembly> PreLoadAssemblies {
        get;
        set;
      } = new List<Assembly>();

      /// <summary>
      /// If a single archetype not being initialized should throw a fatal.
      /// </summary>
      public bool FatalOnCannotInitializeArchetype {
        get;
        set;
      } = false;

      /// <summary>
      /// The prefix to limit assemblies to for loading archetypes
      /// </summary>
      public string ArchetypeAssembliesPrefix {
        get;
        set;
      } = "";

      /// <summary>
      /// The assembly name prefixes to ignore when loading types from assemblies
      /// </summary>
      public List<string> ArchetypeAssemblyPrefixesToIgnore {
        get;
        set;
      } = new List<string> {
        "System.",
        "Microsoft.",
        "Newtonsoft.Json",
        "netstandard",
        "NuGet."
      };

      /// <summary>
      /// How many times to re-run initialization to account for types that require others
      /// </summary>
      public short InitializationAttempts {
        get;
        set;
      } = 10;

      /// <summary>
      /// How many times to attempt to run finalization on remaining initializing types
      /// </summary>
      public short FinalizationAttempts {
        get;
        set;
      } = 1;

      /// <summary>
      /// How many times to loop though components to initialize them accounting for dependency misses
      /// </summary>
      public int ComponentInitializationAttempts {
        get;
        set;
      } = 4;

      /// <summary>
      /// Overrideable bool to allow runtime registrations of types that set AllowSubtypeRuntimeRegistrations to true.
      /// </summary>
      public bool AllowRuntimeTypeRegistrations {
        get;
        set;
      } = false;

      /// <summary>
      /// The location of archetype librararies and mod extensions
      /// </summary>
      public string ModsRootFolderLocation {
        get;
        set;
      } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mods");

      /// <summary>
      /// A pre-settable setting for specifying how to order certain mods for loading.
      /// This will throw if there's a conflict with order.json
      /// </summary>
      public Map<ushort, string> PreOrderedAssemblyFiles {
        get;
        set;
      } = new Map<ushort, string>();

      /// <summary>
      /// Assembled mod load order.
      /// </summary>
      public IReadOnlyList<Assembly> AssemblyLoadOrder
        => _assemblyLoadOrder;
      internal List<Assembly> _assemblyLoadOrder
          = new List<Assembly>();
    }
  }
}