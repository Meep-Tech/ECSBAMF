using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// An ordered collection of delegates
  /// </summary>
  /// <typeparam name="TAction"></typeparam>
  public class DelegateCollection<TAction>
    : OrderedDictionary<string, TAction>
    where TAction : Delegate 
  {

    public DelegateCollection(IEnumerable<KeyValuePair<string, TAction>> orderedValues = null)
      : base(orderedValues) { }

    public static implicit operator DelegateCollection<TAction>(TAction action)
      => new(new KeyValuePair<string, TAction>(0.ToString(), action).AsSingleItemEnumerable());

    public static implicit operator DelegateCollection<TAction>(List<TAction> actions)
      => new(actions.Select((action, index) => new KeyValuePair<string, TAction>(index.ToString(), action)));

    public static implicit operator DelegateCollection<TAction>(Dictionary<string, TAction> actions)
      => new(actions);

    public static implicit operator DelegateCollection<TAction>(TAction[] actions)
      => new(actions.Select((action, index) => new KeyValuePair<string, TAction>(index.ToString(), action)));

    /// <summary>
    /// Change all the delegates and return a new collection
    /// </summary>
    public DelegateCollection<TNewActionType> ReDelegate<TNewActionType>(Func<TAction, TNewActionType> converter)
      where TNewActionType : Delegate
        => new(this.Select(entry => new KeyValuePair<string, TNewActionType>(
          entry.Key,
          converter(entry.Value)
        )));
  }
}