using Meep.Tech.Data.Configuration;
using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

    public partial class Archetype {

    /// <summary>
    /// The base non-generic interface for IBuildOneForEach
    /// </summary>
    public interface ISplayed {
      internal static HashSet<Func<Enumeration, Archetype>> _splayedInterfaceTypesThatAllowLazyInitializations
        = new();
      internal static HashSet<System.Type> _splayedInterfaceTypes
        = new();
      internal static Dictionary<System.Type, Dictionary<System.Type, List<Func<Enumeration, Archetype>>>> _splayedArchetypeCtorsByEnumBaseTypeAndEnumType
        = new();
      internal static Dictionary<System.Type, HashSet<Enumeration>> _completedEnumsByInterfaceBase
        = new();
    }

    /// <summary>
    /// This is a trait that dictates that one of these archetypes should be produced for each item in a given enumeration.
    /// Types extending this cannot be abstract.
    /// This will extend to types that inherit from this archetype, inheriting further from this archetype is not suggested.
    /// The archetype fetched via the System.Type that extends this will be the "splayed" Archetype. You must call ".For()" on it to get a specific sub-archetype specific to one of the enumerations.
    /// </summary>
    public interface ISplayed<TEnumeration, TArchetypeBase> 
      : ITrait<ISplayed<TEnumeration, TArchetypeBase>>,
        ISplayed
        where TEnumeration : Enumeration
        where TArchetypeBase : Archetype
    {

      string ITrait<ISplayed<TEnumeration, TArchetypeBase>>.TraitName
        => "Splayed";

      string ITrait<ISplayed<TEnumeration, TArchetypeBase>>.TraitDescription
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

      internal TArchetypeBase _constructArchetypeFor(TEnumeration enumeration) {
        var subType = ConstructArchetypeFor(enumeration);
        _values[enumeration] = subType;

        return subType;
      }

      /// <summary>
      /// This will be called for each enumeration loaded at runtime for the enumeration type.
      /// TODO: If a new enumeration of the given type is loaded, a new type of this archetype will also try to be constructed here.
      /// </summary>
      internal protected TArchetypeBase ConstructArchetypeFor(TEnumeration enumeration);

      /// <summary>
      /// Get the specific Archetype for an enum value.
      /// </summary>
      public static TArchetypeBase GetSplayedTypeForEnum(TEnumeration enumeration)
        => _values[enumeration];
    }
  }

  /// <summary>
  /// Extensions for types that extend Archetype.IBuildOneForEach[TArchetypeBase, TEnumeration] 
  /// </summary>
  public static class SplayedArchetypedExtensions {

    /// <summary>
    /// Get the specific Archetype for an enum value.
    /// </summary>
    public static TArchetype GetSubTypeForEnum<TArchetype, TEnumeration>(this TArchetype splayedArchetype, TEnumeration enumeration)
      where TArchetype : Archetype, Archetype.ISplayed<TEnumeration, TArchetype>
      where TEnumeration : Enumeration
        => Archetype.ISplayed<TEnumeration, TArchetype>._values[enumeration];
  }
}