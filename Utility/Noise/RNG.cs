using System;

namespace Meep.Tech.Noise {

  /// <summary>
  /// Simple Random generation
  /// </summary>
  public static class RNG {

    /// <summary>
    /// Default static randomness generator
    /// </summary>
    public static System.Random Static {
      get;
    } = new Random(GenerateRandomessSeed());

    /// <summary>
    /// Get the next random int value between and including 0 and 100
    /// </summary>
    public static int NextPercent
      => (int)Math.Round(Static.NextDouble() * 100);

    /// <summary>
    /// Get the next globally unique string id.
    /// </summary>
    public static string NextGuid
      => GenerateNextGuid();

    /// <summary>
    /// Overrideable uniqe id function
    /// </summary>
    public static Func<string> GenerateNextGuid { get; set; }
      = () => Guid.NewGuid().ToString();

    /// <summary>
    /// Used to get the seed for the static randoness function.
    /// </summary>
    public static Func<int> GenerateRandomessSeed { get; set; }
      = () => GenerateNextGuid().GetHashCode();

    #region Random String Extensions

    /// <summary>
    /// Generate a sort of normal random new word.
    /// </summary>
    public static string GenerateRandomNewWord(int? length = null, System.Random random = null) {
      random ??= Static;
      length ??= random.Next(3, 9);
      string[] consonants = { "b", "br", "c", "cr", "d", "dr", "f", "fr", "fn", "g", "gr", "h", "j", "k", "kr", "l", "m", "n", "ng", "p", "pr", "pf", "q", "r", "s", "sr", "st", "sp", "sh", "zh", "t", "th", "v", "w", "x", "z" };
      string[] vowels = { "a", "e", "i", "o", "u", "ae", "y", "oo", "ae" };
      string word = "";
      word += consonants[random.Next(consonants.Length)].ToUpper();
      word += vowels[random.Next(vowels.Length)];
      int lettersAdded = 2;
      while (lettersAdded < length) {
        word += consonants[random.Next(consonants.Length)];
        lettersAdded++;
        word += vowels[random.Next(vowels.Length)];
        lettersAdded++;
      }

      return word;
    }

    #endregion
  }
}
