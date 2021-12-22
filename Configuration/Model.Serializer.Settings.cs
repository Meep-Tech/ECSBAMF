using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KellermanSoftware.CompareNetObjects;

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
          internal set;
        }

        /// <summary>
        /// Helper function to set the default json serializer settings for components.
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
          }
#endif
        };

        /// <summary>
        /// Helper function to set the default json serializer settings for models.
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
          }
#endif
        };

        /// <summary>
        /// Compiled component serializer settings from the settings config function
        /// </summary>
        public JsonSerializerSettings ComponentJsonSerializerSettings {
          get => _componentJsonSerializerSettings 
              ??= ConfigureComponentJsonSerializerSettings(
                new DefaultContractResolver(_universe));
        } JsonSerializerSettings _componentJsonSerializerSettings;

        /// <summary>
        /// Compiled model serializer from the settings config function
        /// </summary>
        public JsonSerializerSettings ModelJsonSerializerSettings {
          get => _modelJsonSerializerSettings 
            ??= ConfigureModelJsonSerializerSettings(
              new DefaultContractResolver(_universe));
        } JsonSerializerSettings _modelJsonSerializerSettings;

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

        /// <summary>
        /// The default config used to compare model objects
        /// </summary>
        public ComparisonConfig DefaultComparisonConfig {
          get;
          set;
        } = new ComparisonConfig {
          AttributesToIgnore = new List<Type> {
            typeof(IsModelComponentsProperty)
          },
          IgnoreObjectTypes = true,
          DifferenceCallback = x => {
            Console.WriteLine("$fgsdfgsdfgsdgf");
          }
        };
      }
    }
  }
}