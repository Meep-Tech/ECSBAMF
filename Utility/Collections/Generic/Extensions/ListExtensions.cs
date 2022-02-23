using System;
using System.Collections.Generic;

namespace Meep.Tech.Collections.Generic {
  public static class ListExtensions {
    public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> original) {
      if (original is null)
        throw new ArgumentNullException(nameof(original));

      return original as IReadOnlyList<T> 
        ?? new ReadOnlyListAdapter<T>(original);
    }

    sealed class ReadOnlyListAdapter<T> : IReadOnlyList<T> {

      public int Count 
        => _values.Count;

      public T this[int index] 
        => _values[index];

      readonly IList<T> _values;

      public ReadOnlyListAdapter(IList<T> source) {
        _values = source;
      }

      public IEnumerator<T> GetEnumerator() 
        => _values.GetEnumerator();

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();
    }
  }
}
