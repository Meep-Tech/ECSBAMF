using Meep.Tech.Data.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Meep.Tech.Data.Archetype;

namespace Meep.Tech.Data {

  public partial class Archetype {

    /// <summary>
    /// The base non-generic interface for IBuildOneForEach
    /// </summary>
    public interface ISplayed {}

    /// <summary>
    /// The base non-generic interface for IBuildOneForEach.Lazily
    /// </summary>
    public interface ISplayedLazily : ISplayed {
      internal static Dictionary<System.Type, Dictionary<System.Type, List<Func<Enumeration, Archetype>>>> _lazySplayedArchetypesByEnumBaseTypeAndEnumType
        = new();
    }

    /// <summary>
    /// This is a trait that dictates that one of these archetypes should be produced for each item in a given enumeration.
    /// Types extending this cannot be abstract.
    /// This will extend to types that inherit from this archetype, inheriting further from this archetype is not suggested.
    /// The archetype fetched via the System.Type that extends this will be the "splayed" Archetype. You must call ".For()" on it to get a specific sub-archetype specific to one of the enumerations.
    /// </summary>
    public interface IBuildOneForEach<TEnumeration, TArchetypeBase> 
      : ITrait<IBuildOneForEach<TEnumeration, TArchetypeBase>>,
        ISplayed
        where TEnumeration : Enumeration
        where TArchetypeBase : Archetype
    {

      /// <summary>
      /// Used to splay the types lazily.
      /// This means the types will not splay on initial load
      /// </summary>
      public interface Lazily : ITrait<Lazily>, ISplayedLazily {
        string ITrait<Lazily>.TraitName
          => "Splayed (Lazy)";

        string ITrait<Lazily>.TraitDescription
          => $"This Archetype was created by a Parent Archetype, along with one other archetype for each Enumeration in: ${typeof(TEnumeration).FullName}. This archetype's Associated Enum is: {AssociatedEnum}. The splayed types will NOT be built on initial load.";
        
        internal static Dictionary<TEnumeration, TArchetypeBase> _values
          = new();

        /// <summary>
        /// the enum associated with this archetype.
        /// </summary>
        TEnumeration AssociatedEnum {
          get;
          internal protected set;
        }

        /// <summary>
        /// This will be called for each enumeration loaded at runtime for the enumeration type.
        /// TODO: If a new enumeration of the given type is loaded, a new type of this archetype will also try to be constructed here.
        /// </summary>
        internal protected TArchetypeBase ConstructArchetypeFor(TEnumeration enumeration);
      }

      string ITrait<IBuildOneForEach<TEnumeration, TArchetypeBase>>.TraitName
        => "Splayed";

      string ITrait<IBuildOneForEach<TEnumeration, TArchetypeBase>>.TraitDescription
        => $"This Archetype was created by a Parent Archetype, along with one other archetype for each Enumeration in: ${typeof(TEnumeration).FullName}. This archetype's Associated Enum is: {AssociatedEnum}.";

      internal static Dictionary<TEnumeration, TArchetypeBase> _values
        = new();

      /// <summary>
      /// the enum associated with this archetype.
      /// </summary>
      TEnumeration AssociatedEnum {
        get;
        internal protected set;
      }

      /// <summary>
      /// This will be called for each enumeration loaded at runtime for the enumeration type.
      /// TODO: If a new enumeration of the given type is loaded, a new type of this archetype will also try to be constructed here.
      /// </summary>
      internal protected TArchetypeBase ConstructArchetypeFor(TEnumeration enumeration);
    }
  }

  /// <summary>
  /// Extensions for types that extend Archetype.IBuildOneForEach[TArchetypeBase, TEnumeration] 
  /// </summary>
  public static class SplayedArchetypedExtensions {

    /// <summary>
    /// Get the specific Archetype for an enum value.
    /// </summary>
    public static TArchetype For<TArchetype, TEnumeration>(this TArchetype splayedArchetype, TEnumeration enumeration)
      where TArchetype : Archetype, Archetype.IBuildOneForEach<TEnumeration, TArchetype>
      where TEnumeration : Enumeration
        => Archetype.IBuildOneForEach<TEnumeration, TArchetype>._values[enumeration];

    /// <summary>
    /// Get the specific Archetype for an enum value.
    /// </summary>
    public static TLazyArchetype For<TLazyArchetype, TEnumeration>(this ISplayedLazily splayedArchetype, TEnumeration enumeration)
      where TLazyArchetype : Archetype, Archetype.ISplayedLazily
      where TEnumeration : Enumeration
        => Archetype.IBuildOneForEach<TEnumeration, TLazyArchetype>.Lazily._values[enumeration];
  }
}