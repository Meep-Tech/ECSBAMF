using Newtonsoft.Json;
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

        internal Universe _universe {
          get;
          set;
        }

        /// <summary>
        /// The db context used by the serializer
        /// </summary>
        public DbContext DbContext {
          get;
          set;
        }

        /// <summary>
        /// Json serializer settings for easy Component serialization
        /// </summary>
        public Func<DefaultContractResolver, JsonSerializerSettings> ConfigureComponentJsonSerializerSettings {
          get;
          set;
        } = defaultResolver => new JsonSerializerSettings {
          ContractResolver = defaultResolver,
          Formatting = Formatting.Indented
#if DEBUG
          ,Error = (sender, args) =>
          {
            if(System.Diagnostics.Debugger.IsAttached) {
              System.Diagnostics.Debugger.Break();
            }
          },
          MissingMemberHandling = MissingMemberHandling.Error
#endif
        };

        /// <summary>
        /// Json serializer settings for easy Component serialization
        /// </summary>
        public Func<DefaultContractResolver, JsonSerializerSettings> ConfigureModelJsonSerializerSettings {
          get;
          set;
        } = defaultResolver => new JsonSerializerSettings {
          ContractResolver = defaultResolver,
          Formatting = Formatting.Indented
#if DEBUG
          ,
          Error = (sender, args) => {
            if(System.Diagnostics.Debugger.IsAttached) {
              System.Diagnostics.Debugger.Break();
            }
          },
          MissingMemberHandling = MissingMemberHandling.Error
#endif
        };

        /// <summary>
        /// Compiled component serializer from the above settings
        /// </summary>
        public JsonSerializerSettings ComponentJsonSerializerSettings {
          get => _componentJsonSerializerSettings 
              ??= ConfigureComponentJsonSerializerSettings(
                new DefaultContractResolver(_universe));
        } JsonSerializerSettings _componentJsonSerializerSettings;

        /// <summary>
        /// Compiled component serializer from the above settings
        /// </summary>
        public JsonSerializerSettings ModelJsonSerializerSettings {
          get => _modelJsonSerializerSettings 
            ??= ConfigureModelJsonSerializerSettings(
              new DefaultContractResolver(_universe));
        } JsonSerializerSettings _modelJsonSerializerSettings;

        /// <summary>
        /// Compiled component serializer from the above settings
        /// </summary>
        public JsonSerializer ComponentJsonSerializer {
          get => _componentJsonSerializer ??= JsonSerializer
            .Create(ConfigureComponentJsonSerializerSettings(
              new DefaultContractResolver(_universe)));
        } JsonSerializer _componentJsonSerializer;

        /// <summary>
        /// Compiled component serializer from the above settings
        /// </summary>
        public JsonSerializer ModelJsonSerializer {
          get => _modelJsonSerializer ??= JsonSerializer
            .Create(ConfigureModelJsonSerializerSettings(
              new DefaultContractResolver(_universe)));
        } JsonSerializer _modelJsonSerializer;

        /// <summary>
        /// The types to map to the db context.
        /// You can provide a config function if you want, but don't have to.
        /// </summary>
        public Dictionary<System.Type, Action<EntityTypeBuilder>> TypesToMapToDbContext {
          get;
        } = new Dictionary<System.Type, Action<EntityTypeBuilder>>();

        /// <summary>
        /// If true, properies need to opt out to avoid being serialized into json using JsonIgnore. Even private properties.
        /// </summary>
        public bool PropertiesMustOptOutForJsonSerialization {
          get;
          set;
        } = true;
      }
    }
  }
}