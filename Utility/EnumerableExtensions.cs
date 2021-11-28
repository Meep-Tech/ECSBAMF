using System;
using System.Collections.Generic;
using System.Text;

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
  }
}
