using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// Dictionaty extensions specific to xbam components
  /// </summary>
  public static class XbamSpecificDictionaryExtensions {

    /// <summary>
    /// Append a component to a dictionary and return the collection
    /// </summary>
    public static Dictionary<string, TComponentBase> Append<TComponentBase>(this Dictionary<string, TComponentBase> current, TComponentBase component)
      where TComponentBase : IComponent {
      current.Add(component.Key, component);
      return current;
    }

    /// <summary>
    /// Append a value to a dictionary and return the collection
    /// </summary>
    public static HashSet<Func<IBuilder, IModel.IComponent>> Append(this HashSet<Func<IBuilder, IModel.IComponent>> current, Func<IBuilder, IModel.IComponent> value) {
      current.Add(value);
      return current;
    }
  }
}
