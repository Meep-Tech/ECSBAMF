using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq.Expressions;

namespace Meep.Tech.Data {

  public abstract partial class Archetype<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> 
  {
    /// <summary>
    /// Used to convert an Archetype to a general string for storage
    /// </summary>
    public new class ToKeyStringConverter : ValueConverter<TArchetypeBase, string> {
      
      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public ToKeyStringConverter() :
        base(_convertToProviderExpression, _convertFromProviderExpression) {
      }

      private static readonly Expression<Func<string, TArchetypeBase>> _convertFromProviderExpression 
        = x => ToArchetype(x);

      private static readonly Expression<Func<TArchetypeBase, string>> _convertToProviderExpression
        = x => ToString(x);

      static TArchetypeBase ToArchetype(string key) {
        return key.Split("@") is string[] parts
          ? parts.Length == 1
            ? (TArchetypeBase)Archetypes.Id[key].Archetype
            : parts.Length == 2
              ? (TArchetypeBase)Universe.Get(parts[1]).Archetypes.Id[parts[2]].Archetype
              : throw new ArgumentException("ArchetypeKey")
          : throw new ArgumentNullException("ArchetypeKey");
      }

      static string ToString(TArchetypeBase archetype)
        => archetype.Id.Key + (!string.IsNullOrEmpty(archetype.Id.Universe.Key)
          ? "@" + archetype.Id.Universe.Key
          : "");
    }
  }
}
