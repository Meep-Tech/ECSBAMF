namespace Meep.Tech.Data {

  public abstract partial class Archetype {

    /// <summary>
    /// An Id unique to each Archetype.
    /// Can be used as a static key.
    /// This is a base, non-absract class for utility.
    /// </summary>
    public abstract class Identity : Enumeration<Identity> {

      /// <summary>
      /// The Name of this Identity.
      /// By default, this is used to generate the key.
      /// </summary>
      public string Name { 
        get;
      }

      /// <summary>
      /// A Univerally Unique Key for this Achetype Identity.
      /// </summary>
      public string Key {
        get => _castKey ??= ExternalId as string;
      } string _castKey;

      /// <summary>
      /// The archetype this id is for
      /// </summary>
      public Archetype Archetype {
        get;
        internal set;
      }

      /// <summary>
      /// Can be used as an internal value to index this identity.
      /// May change between runtimes/runs of a program.
      /// </summary>
      public int InternalIndex
        => InternalId;

      /// <summary>
      /// The universe this identity is a part of
      /// </summary>
      public Universe Universe {
        get;
        internal set;
      }

      /// <summary>
      /// Make a new ID.
      /// </summary>
      /// <param name="name"></param>
      /// <param name="key"></param>
      protected Identity(
        string name,
        string key = null
      ) : base(key ?? name) {
        Name = name;
      }
    }
  }

  /// <summary>
  /// An Id unique to each Archetype.
  /// Can be used as a static key.
  /// </summary>
  public partial class Archetype<TModelBase, TArchetypeBase>
    : Archetype
    where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {

    /// <summary>
    /// An Id unique to each Archetype.
    /// Can be used as a static key.
    /// </summary>
    public new class Identity : Archetype.Identity {

      /// <summary>
      /// A base key string to use instead of the model base type name
      /// </summary>
      public static string BaseKeyString {
        get;
        set;
      }

      /// <summary>
      /// Make a new identiy for this Archetype Base Type
      /// </summary>
      /// <param name="name">Used to generate the final part of the key. Spaces are removed before then.</param>
      /// <param name="keyPrefixEndingAdditions">Added to the key right before the end here: Type..{keyPrefixEndingAdditions}.name</param>
      public Identity(string name, string keyPrefixEndingAdditions = null) 
        : base(name, $"{BaseKeyString ?? typeof(TModelBase).FullName}.{keyPrefixEndingAdditions ?? ""}{(string.IsNullOrEmpty(keyPrefixEndingAdditions) ? "" : ".")}{name}") {}
    }
  }
}
