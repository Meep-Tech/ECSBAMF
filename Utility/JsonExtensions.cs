using Newtonsoft.Json.Linq;
using System;

namespace Meep.Tech.Data {
  public static class JsonExtensions {

    /// <summary>
    /// Try to get a value by type, case insensitive by default.
    /// </summary>
    public static T GetValue<T>(this JObject jObject, string property, StringComparison comparer = StringComparison.OrdinalIgnoreCase, string errorMessageOverride = null) {
      if (jObject.TryGetValue(property, comparer, out JToken valueToken)) {
        return valueToken.Value<T>();
      }

      throw new ArgumentException(errorMessageOverride ?? $"Property {property} not found in JObject. {(comparer == StringComparison.OrdinalIgnoreCase ? " Case Insensitive Search Applied." : "")}");
    }

    /// <summary>
    /// Try to get a value by type, case insensitive by default.
    /// </summary>
    public static T TryGetValue<T>(this JObject jObject, string property, StringComparison comparer = StringComparison.OrdinalIgnoreCase, T @default = default) {
      if (jObject.TryGetValue(property, comparer, out JToken valueToken)) {
        return valueToken.Value<T>();
      }

      return @default;
    }
  }
}
