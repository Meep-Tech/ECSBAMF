using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Universe {

    /// <summary>
    /// Data pertaining to enumerations
    /// </summary>
    public class EnumerationData {

      /// <summary>
      /// All enumerations indexed by type.
      /// GetAllForType is faster.
      /// </summary>
      public IReadOnlyDictionary<System.Type, IReadOnlyList<Enumeration>> ByType
        => _byType.ToDictionary(
          e => typeof(Enumeration).Assembly.GetType(e.Key),
          e => (IReadOnlyList<Enumeration>)e.Value.Values.ToList()
        ); readonly Dictionary<string, Dictionary<object, Enumeration>> _byType
          = new Dictionary<string, Dictionary<object, Enumeration>>();

      /// <summary>
      /// Get all enumerations of a given type
      /// </summary>
      public IEnumerable<Enumeration> GetAllByType(System.Type type)
        => GetAllByType(type.FullName);

      /// <summary>
      /// Get all enumerations of a given type
      /// </summary>
      public IEnumerable<Enumeration> GetAllByType(string typeFullName)
        => _byType[typeFullName].Values;

      /// <summary>
      /// Get all enumerations of a given type
      /// </summary>
      public IEnumerable<Enumeration> GetAllByType<TEnumeration>()
        where TEnumeration : Enumeration
          => GetAllByType(typeof(TEnumeration));

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public Enumeration Get(string typeFullName, object externalId)
          => _byType[typeFullName][externalId];

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public TEnumeration Get<TEnumeration>(object externalId)
        where TEnumeration : Enumeration
          => (TEnumeration)Get(typeof(TEnumeration).FullName, externalId);

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public Enumeration Get(System.Type enumType, object externalId)
          => Get(enumType.FullName, externalId);

      internal void _register(Enumeration enumeration) {
        if(_byType.TryGetValue(enumeration.GetType().FullName, out var found)) {
          found.Add(
            enumeration.ExternalId,
            enumeration
          );
        }
        else
          _byType[enumeration.GetType().FullName] = new Dictionary<object, Enumeration> {
            {enumeration.ExternalId, enumeration }
          };
      }
    }
  }
}
