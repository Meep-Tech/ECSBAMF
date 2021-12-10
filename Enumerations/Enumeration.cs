using System;
using System.Collections.Generic;
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
    /// The current number of enums. Used for internal indexing.
    /// </summary>
    static int CurrentMaxInternalEnumId 
      = 0;

    /// <summary>
    /// The assigned internal id of this archetype. This is only consistend within the current runtime and execution.
    /// </summary>
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
    /// Make an archetype ID
    /// </summary>
    protected Enumeration(object uniqueIdentifier) {
      // Remove any spaces:
      ExternalId = Regex.Replace($"{uniqueIdentifier}", @"\s+", "");
      InternalId = Interlocked.Increment(ref CurrentMaxInternalEnumId) - 1;
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
    where TEnumBase : Enumeration<TEnumBase> {

    /// <summary>
    /// Readonly list of all items
    /// </summary>
    public static IReadOnlyList<Enumeration<TEnumBase>> All
      => _all;

    /// <summary>
    /// Intenral list of all items
    /// </summary>
    readonly static List<Enumeration<TEnumBase>> _all
      = new List<Enumeration<TEnumBase>>();

    /// <summary>
    /// Ctor add to all.
    /// </summary>
    protected Enumeration(object uniqueIdentifier) 
      : base(uniqueIdentifier) {
      _all.Add(this);
    }
  }
}
