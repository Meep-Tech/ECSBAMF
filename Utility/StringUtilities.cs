namespace Meep.Tech.Data.Utility {

  /// <summary>
  /// Some string utilities i use a lot
  /// </summary>
  public static class StringUtilities {

    /// <summary>
    /// Limit a string to a max size, with or without elipses.
    /// </summary>
    public static string LimitTo(this string value, int maxSize, bool withElipsies = true) {
      if ((value.Length + (withElipsies ? 3 : 0)) >= maxSize) {
        value = value[0..(withElipsies ? maxSize - 3 : maxSize)];
      }
      return withElipsies ? value + "..." : value;
    }
  }
}
