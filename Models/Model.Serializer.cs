using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meep.Tech.Data {

  public partial class Model {
    public partial class Serializer {

      /// <summary>
      /// Settings for the Serializer
      /// </summary>
      public class Settings {

        /// <summary>
        /// Json serializer settings for easy Component serialization
        /// </summary>
        public JsonSerializerSettings ComponentJsonSerializerSettings {
          get;
          set;
        }

        /// <summary>
        /// Compiled component serializer from the above settings
        /// </summary>
        public JsonSerializer ComponentJsonSerializer {
          get => _componentJsonSerializer ??= JsonSerializer.Create(ComponentJsonSerializerSettings);
        } JsonSerializer _componentJsonSerializer;

        /// <summary>
        /// The types to map to the db context.
        /// You can provide a config function if you want, but don't have to.
        /// </summary>
        public Dictionary<System.Type, Action<EntityTypeBuilder>> TypesToMapToDbContext {
          get;
        } = new Dictionary<System.Type, Action<EntityTypeBuilder>>();

        /// <summary>
        /// The db context used by the serializer
        /// </summary>
        public DbContext DbContext {
          get;
          set;
        }
      }

      /// <summary>
      /// The serializer options
      /// </summary>
      public Settings Options {
        get;
      }

      /// <summary>
      /// Make a new serializer for a universe
      /// </summary>
      /// <param name="options"></param>
      internal Serializer(Settings options = null) {
        Options = options ?? new Settings();
      }
    }
  }
}