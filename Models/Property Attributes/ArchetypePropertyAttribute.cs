using System;

namespace Meep.Tech.Data {

  /// <summary>
  /// Marks a model property field as the Archetype field.
  /// Used for serialization methods
  /// </summary>
  public class ArchetypePropertyAttribute : Attribute {
    public ArchetypePropertyAttribute()
      : base() { }
  }
}
