using Meep.Tech.Collections.Generic;
using Meep.Tech.Data.Configuration;
using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// A global collection of arechetypes.
  /// This is what the loader builds.
  /// </summary>
  public sealed partial class Universe {

    /// <summary>
    /// All Universes
    /// </summary>
    public static IReadOnlyDictionary<string, Universe> s
      => _all;
    static OrderedDictionary<string, Universe> _all
      = new();

    /// <summary>
    /// The default archetype universe
    /// </summary>
    public static Universe Default
      => Data.Archetypes.DefaultUniverse;

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
    /// The extra contexts
    /// </summary>
    public ExtraContextsData ExtraContexts {
      get;
    }

    /// <summary>
    /// Make a new universe of Archetypes
    /// </summary>
    public Universe(Loader loader, string nameKey = null) {
      Key = nameKey ?? Key;
      Loader = loader;
      Loader.Universe = this;
      Archetypes = new(this);
      Models = new(this);
      Components = new(this);
      Enumerations = new(this);
      ExtraContexts = new ExtraContextsData(this);

      // set this as the default universe if there isn't one yet
      Data.Archetypes.DefaultUniverse ??= this;
      _all.Add(Key, this);
    }

    /// <summary>
    /// Get an extra context item that was assigned to this universe.
    /// </summary>
    public void SetExtraContext<TExtraContext>(TExtraContext extraContext)
      where TExtraContext : ExtraContext 
    {
      if(Loader.IsFinished) {
        throw new Exception($"Must add extra context before the loader for the universe has finished.");
      }
      extraContext.Universe = this;
      ExtraContexts._extraContexts[typeof(TExtraContext)] = extraContext;

      ExtraContexts._addAllOverrideDelegates(extraContext);
    }

    /// <summary>
    /// Get an extra context item that was assigned to this universe.
    /// </summary>
    public TExtraContext GetExtraContext<TExtraContext>()
      where TExtraContext : ExtraContext {
      try {
        return (TExtraContext)ExtraContexts._extraContexts[typeof(TExtraContext)];
      } catch (System.Collections.Generic.KeyNotFoundException keyNotFoundE) {
        throw new KeyNotFoundException($"No extra context of the type {typeof(TExtraContext).FullName} added to this universe. Further ECSBAM configuration may be required.", keyNotFoundE);
      }
    }
  }
}
