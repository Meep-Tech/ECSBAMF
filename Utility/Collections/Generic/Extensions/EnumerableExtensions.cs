using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  public static class EnumerableExtensions {

    /// <summary>
    /// Turn one item into an enumerable contaiing itself.
    /// </summary>
    public static IEnumerable<T> AsSingleItemEnumerable<T>(this T singleItem)
      => new[] { singleItem };

    /// <summary>
    /// do on each
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> @do) {
      foreach(T @value in enumeration) {
        @do(value);
      }
    }

    /// <summary>
    /// append a value if the condition is true
    /// </summary>
    public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumeration, Func<bool> @if, T item) 
      => @if() ? enumeration.Append(item) : enumeration;

    /// <summary>
    /// append a value if the condition is true
    /// </summary>
    public static IEnumerable<T> ConcatIf<T>(this IEnumerable<T> enumeration, Func<bool> @if, IEnumerable<T> other) 
      => @if() ? enumeration.Concat(other) : enumeration;

    /// <summary>
    /// append a value if the condition is true
    /// </summary>
    public static IEnumerable<T> ConcatIfNotNull<T>(this IEnumerable<T> enumeration, IEnumerable<T> other) 
      => other is not null ? enumeration.Concat(other) : enumeration;

    /// <summary>
    /// Reverse the list.
    /// </summary>
    public static IEnumerable<T> Reverse<T>(this IList<T> list) {
      for(int i = list.Count - 1; i >= 0; i--) {
        yield return list[i];
      }
    }

    /// <summary>
    /// Get the values until the desired index. Not including it.
    /// </summary>
    public static IEnumerable<T> Until<T>(this IEnumerable<T> list, int index) {
      int i = 0;
      var enumerator = list.GetEnumerator();
      while(i < index && enumerator.MoveNext()) {
        yield return enumerator.Current;
        i++;
      }
    }
  }
}
