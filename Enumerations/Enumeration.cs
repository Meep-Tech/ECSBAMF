using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Meep.Tech.Data {

  /// <summary>
  /// Base for a simple Enumerable value
  /// </summary>
  public abstract partial class Enumeration 
    : IEquatable<Enumeration> 
  {

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
      if(Universe is null) {
        throw new System.ArgumentNullException(nameof(Universe));
      }
      if(Universe.Enumerations is null) {
        throw new System.ArgumentNullException("Universe.Enumerations");
      }

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
      => (a is null && b is null) || (a?.Equals(b) ?? false);
     
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
    public static IEnumerable<TEnumBase> All
      => Archetypes.DefaultUniverse.Enumerations.GetAllByType<TEnumBase>().Cast<TEnumBase>();

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
