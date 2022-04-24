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
    public static Dictionary<string, IModel.IComponent> Append<TComponentBase>(this Dictionary<string, IModel.IComponent> current, TComponentBase component)
      where TComponentBase : IModel.IComponent {
      current.Add(component.Key, component);
      return current;
    }

    /// <summary>
    /// Append a component to a dictionary and return the collection
    /// </summary>
    public static Dictionary<string, Archetype.IComponent> Append<TComponentBase>(this Dictionary<string, Archetype.IComponent> current, TComponentBase component)
      where TComponentBase : Archetype.IComponent {
      current.Add(component.Key, component);
      return current;
    }
    
    /// <summary>
    /// Append a component to a dictionary and return the collection
    /// </summary>
    public static Dictionary<string, IModel.IComponent> Update<TComponentBase>(this Dictionary<string, IModel.IComponent> current, TComponentBase component)
      where TComponentBase : IModel.IComponent {
      current[component.Key] = component;
      return current;
    }
    
    /// <summary>
    /// Append a component to a dictionary and return the collection
    /// </summary>
    public static Dictionary<string, Archetype.IComponent> Update<TComponentBase>(this Dictionary<string, Archetype.IComponent> current, TComponentBase component)
      where TComponentBase : Archetype.IComponent {
      current[component.Key] = component;
      return current;
    }

    /// <summary>
    /// Append a component to a dictionary and return the collection
    /// </summary>
    public static Dictionary<string, IModel.IComponent> Update<TComponentBase>(this Dictionary<string, IModel.IComponent> current, Func<TComponentBase, TComponentBase> updateComponent)
      where TComponentBase : IModel.IComponent<TComponentBase> {
      current[Components<TComponentBase>.Key] = updateComponent((TComponentBase)current[Components<TComponentBase>.Key]);
      return current;
    }

    /// <summary>
    /// Append a component to a dictionary and return the collection
    /// </summary>
    public static Dictionary<string, Archetype.IComponent> Update<TComponentBase>(this Dictionary<string, Archetype.IComponent> current, Func<TComponentBase, TComponentBase> updateComponent)
      where TComponentBase : Archetype.IComponent<TComponentBase> {
      current[Components<TComponentBase>.Key] = updateComponent((TComponentBase)current[Components<TComponentBase>.Key]);
      return current;
    }

    /// <summary>
    /// Append a value to a hash set collection and return it
    /// </summary>
    public static HashSet<Func<IBuilder, IModel.IComponent>> Append(this HashSet<Func<IBuilder, IModel.IComponent>> current, Func<IBuilder, IModel.IComponent> value) {
      current.Add(value);
      return current;
    }
  }
}
