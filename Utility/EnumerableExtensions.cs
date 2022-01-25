using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {
  public static class EnumerableExtensions {

    /// <summary>
    /// do on each
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> @do) {
      foreach(T @value in enumeration) {
        @do(value);
      }
    }

    public static IEnumerable<T> Reverse<T>(this IList<T> list) {
      for(int i = list.Count - 1; i >= 0; i--) {
        yield return list[i];
      }
    }
  }
}
