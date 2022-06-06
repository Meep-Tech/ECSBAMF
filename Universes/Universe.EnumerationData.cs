using Meep.Tech.Data.Reflection;
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
          e => TypeExtensions.GetTypeByFullName(e.Key),
          e => (IReadOnlyList<Enumeration>)e.Value.Values.ToList()
        ); readonly Dictionary<string, Dictionary<object, Enumeration>> _byType
          = new();

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

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public bool TryToGet(System.Type enumType, object externalId, out Enumeration found) {
        if (_byType.TryGetValue(enumType.FullName, out var _found)) {
          if (_found.TryGetValue(externalId, out found)) {
            return true;
          }
        }

        found = null;
        return false;
      }

      /// <summary>
      /// Get the enumerations of the given type with the given external id
      /// </summary>
      public bool TryToGet<TEnumeration>(object externalId, out TEnumeration found) where TEnumeration : Enumeration {
        if (_byType.TryGetValue(typeof(TEnumeration).FullName, out var _found)) {
          if (_found.TryGetValue(externalId, out var foundEnum)) {
            found = foundEnum as TEnumeration;
            return foundEnum is not null;
          }
        }

        found = null;
        return false;
      }

      internal void _register(Enumeration enumeration) {
        var enumType = enumeration.GetType();
        while (enumType.IsAssignableToGeneric(typeof(Enumeration<>)) && (!enumType.IsGenericType || (enumType.GetGenericTypeDefinition() != typeof(Enumeration<>)))) {
          if(_byType.TryGetValue(enumType.FullName, out var found)) {
            found.Add(
              enumeration.ExternalId,
             enumeration
            );
          }
          else
            _byType[enumType.FullName] = new Dictionary<object, Enumeration> {
              {enumeration.ExternalId, enumeration }
            };

          enumType = enumType.BaseType;
        }
      }

      internal void _deRegister(Enumeration enumeration) {
        var enumType = enumeration.GetType();
        while (enumType.IsAssignableToGeneric(typeof(Enumeration<>)) && (!enumType.IsGenericType || (enumType.GetGenericTypeDefinition() != typeof(Enumeration<>)))) {
          if(_byType.TryGetValue(enumType.FullName, out var found)) {
            found.Remove(enumeration.ExternalId);
          }

          enumType = enumType.BaseType;
        }
      }
    }
  }
}
