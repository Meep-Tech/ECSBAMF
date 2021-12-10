using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace Meep.Tech.Data {

  public partial class Model {
    public static partial class Serializer {

      /// <summary>
      /// Settings for the Serializer
      /// </summary>
      public static class Settings {

        /// <summary>
        /// Json serializer settings for easy Component serialization
        /// </summary>
        public static JsonSerializerSettings ComponentJsonSerializerSettings {
          get;
          internal set;
        }

        /// <summary>
        /// Compiled component serializer from the above settings
        /// </summary>
        public static JsonSerializer ComponentJsonSerializer {
          get => _componentJsonSerializer ??= JsonSerializer.Create(ComponentJsonSerializerSettings);
        } static JsonSerializer _componentJsonSerializer;

        /// <summary>
        /// The db context used by the serializer
        /// </summary>
        public static DbContext DbContext {
          get;
          internal set;
        }
      }
    }
  }
}