using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Meep.Tech.Data {

  /// <summary>
  /// Base for a simple Enumerable value
  /// </summary>
  public abstract class Enumeration 
    : IEquatable<Enumeration> 
  {

    /// <summary>
    /// Json Converter for Enumerations
    /// </summary>
    public class JsonConverter : Newtonsoft.Json.JsonConverter<Enumeration> {
      public override Enumeration ReadJson(JsonReader reader, Type objectType, [AllowNull] Enumeration existingValue, bool hasExistingValue, JsonSerializer serializer) {
        JObject value = serializer.Deserialize<JObject>(reader);
        string key = value.Value<string>(Model.Serializer.EnumTypePropertyName);
        string[] parts = key.Split('@');
        Universe universe = parts.Length == 1 
          ? Archetypes.DefaultUniverse 
          : Universe.Get(parts.Last());

        return universe.Enumerations.Get(
          parts.First(),
          value.Value<object>("externalId")
        );
      }

      public override void WriteJson(JsonWriter writer, [AllowNull] Enumeration value, JsonSerializer serializer) {
        serializer.Converters.Remove(this);
        JObject serialized = JObject.FromObject(value, serializer);
        serializer.Converters.Add(this);
        string key = value.Universe.Key.Equals(Archetypes.DefaultUniverse.Key)
            ? $"{value.EnumBaseType.FullName}@{value.Universe.Key}"
            : value.EnumBaseType.FullName;

        serialized.Add(
          Model.Serializer.EnumTypePropertyName,
          key
        );
        serializer.Serialize(writer, serialized);
      }
    }

    /// <summary>
    /// The current number of enums. Used for internal indexing.
    /// </summary>
    static int CurrentMaxInternalEnumId 
      = 0;

    /// <summary>
    /// The assigned internal id of this archetype. This is only consistend within the current runtime and execution.
    /// </summary>
    [JsonIgnore]
    public int InternalId {
      get;
    }

    /// <summary>
    /// The perminant and unique external id
    /// </summary>
    public object ExternalId {
      get;
    }

    /// <summary>
    /// The universe this enum is a part of
    /// </summary>
    [JsonIgnore]
    public Universe Universe {
      get;
    }

    /// <summary>
    /// The base type of this enumeration
    /// </summary>
    public abstract Type EnumBaseType {
      get;
    }

    /// <summary>
    /// Make an archetype ID
    /// </summary>
    protected Enumeration(object uniqueIdentifier, Universe universe = null) {
      // Remove any spaces:
      ExternalId = Regex.Replace($"{uniqueIdentifier}", @"\s+", "");
      InternalId = Interlocked.Increment(ref CurrentMaxInternalEnumId) - 1;
      Universe = universe ?? Archetypes.DefaultUniverse;

      Universe.Enumerations._register(this);
    }

    #region Equality, Comparison, and Conversion

    /// <summary>
    /// ==
    /// </summary>
    /// 
    public override bool Equals(object obj)
      => Equals(obj as Enumeration);

    public bool Equals(Enumeration other) 
      => !(other is null) && other.ExternalId == ExternalId;

    public static bool operator ==(Enumeration a, Enumeration b)
      => (a is null && b is null) || a.Equals(b);

    public static bool operator !=(Enumeration a, Enumeration b)
      => !(a == b);

    /// <summary>
    /// #
    /// </summary>
    public override int GetHashCode() {
      // TODO: test using internal id here instead for more speeeed in indexing:
      return ExternalId.GetHashCode();
    }

    public override string ToString() {
      return ExternalId.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Base for a general Enum
  /// </summary>
  /// <typeparam name="TEnumBase"></typeparam>
  public abstract class Enumeration<TEnumBase>
    : Enumeration
    where TEnumBase : Enumeration<TEnumBase>
  {

    /// <summary>
    /// Readonly list of all items from the default universe
    /// </summary>
    public static IEnumerable<Enumeration<TEnumBase>> All
      => Archetypes.DefaultUniverse.Enumerations.GetAllByType<TEnumBase>().Cast<Enumeration<TEnumBase>>();

    /// <summary>
    /// The base type of this enumeration
    /// </summary>
    public override Type EnumBaseType
      => typeof(TEnumBase);

    /// <summary>
    /// Ctor add to all.
    /// </summary>
    protected Enumeration(object uniqueIdentifier, Universe universe = null) 
      : base(uniqueIdentifier, universe) {}
  }
}
