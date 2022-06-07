using System.Collections.Generic;

namespace Meep.Tech.Data {

  public partial class Archetype<TModelBase, TArchetypeBase> {

    /// <summary>
    /// This is a trait that dictates that one of these archetypes should be produced for each item in a given enumeration.
    /// Types extending this cannot be abstract.
    /// This will extend to types that inherit from this archetype, inheriting further from this archetype is not suggested.
    /// The archetype fetched via the System.Type that extends this will be the "splayed" Archetype. You must call ".For()" on it to get a specific sub-archetype specific to one of the enumerations.
    /// </summary>
    public interface IBuildOneForEach<TEnumeration> 
      : IFactory
        where TEnumeration : Enumeration
    {
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
      /// TODO: This will be called for each enumeration loaded at runtime for the enumeration type.
      /// TODO: If a new enumeration of the given type is loaded, a new type of this archetype will also try to be constructed here.
      /// </summary>
      TArchetypeBase ConstructArchetypeFor(TEnumeration enumeration);

    }
  }

  /// <summary>
  /// Extensions for types that extend Archetype.IBuildOneForEach[TArchetypeBase, TEnumeration] 
  /// </summary>
  public static class SplayedArchetypedExtensions {

      /// <summary>
      /// Get the specific Archetype for an enum value.
      /// </summary>
      public static TArchetypeBase For<TArchetype, TArchetypeBase, TModelBase, TEnumeration>(this TArchetype splayedArchetype, TEnumeration enumeration)
        where TArchetype : Archetype<TModelBase, TArchetypeBase>.IBuildOneForEach<TEnumeration>
        where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
        where TModelBase : IModel<TModelBase>
        where TEnumeration : Enumeration
          => Archetype<TModelBase, TArchetypeBase>.IBuildOneForEach<TEnumeration>._values[enumeration];
  }
}