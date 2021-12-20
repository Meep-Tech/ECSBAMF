using System;

namespace Meep.Tech.Data {
  public static class RNG {

    /// <summary>
    /// Access to a static randomness instance.
    /// </summary>
    public static class Static {

      /// <summary>
      /// Default static randomness generator
      /// </summary>
      static System.Random _defaultStatic {
        get;
      } = new Random(GetDefaultStaticRandomessSeed());

      /// <summary>
      /// Get the next value from a static data source:
      /// </summary>
      /// <param name="min">inclusive</param>
      /// <param name="max">exclusive</param>
      /// <returns>a random int within the inclusive bounds</returns>
      public static Func<int, int, int> GetNextStaticInt { get; set; }
        = (min, max) => _defaultStatic.Next(min, max);

      /// <summary>
      /// Get the next value from a static data source:
      /// </summary>
      /// <param name="min">inclusive</param>
      /// <param name="max">exclusive</param>
      /// <returns>a random int within the inclusive bounds</returns>
      public static Func<int> GetDefaultStaticRandomessSeed { get; set; }
        = () => Guid.NewGuid().GetHashCode();

      /// <summary>
      /// GetNextStaticInt syntax helper.
      /// <param name="min">inclusive</param>
      /// <param name="max">exclusive</param>
      /// </summary>
      public static int Next(int min = int.MinValue, int max = int.MaxValue)
        => GetNextStaticInt(min, max);
    }

    /// <summary>
    /// Overrideable uniqe id function
    /// </summary>
    public static Func<string> GetNextUniqueId { get; set; }
      = () => Guid.NewGuid().ToString();
  }
}
