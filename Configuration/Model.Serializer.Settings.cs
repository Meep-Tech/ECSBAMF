using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using KellermanSoftware.CompareNetObjects;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Model {
    public partial class Serializer {

      /// <summary>
      /// Settings for the Model Serializer
      /// </summary>
      public class Settings {

        internal Universe _universe {
          get;
          set;
        }

        /// <summary>
        /// Helper function to set the default json serializer settings for components.
        /// </summary>
        public Func<DefaultContractResolver, IEnumerable<JsonConverter>, JsonSerializerSettings> ConfigureComponentJsonSerializerSettings {
          get;
          set;
        } = (defaultResolver, defaultConverters) => new JsonSerializerSettings {
          ContractResolver = defaultResolver,
          Formatting = Formatting.Indented,
          Converters = defaultConverters.ToList()
#if DEBUG
          ,
          Error = (sender, args) =>
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
        public Func<DefaultContractResolver, IEnumerable<JsonConverter>, JsonSerializerSettings> ConfigureModelJsonSerializerSettings {
          get;
          set;
        } = (defaultResolver, defaultConverters) => new JsonSerializerSettings {
          ContractResolver = defaultResolver,
          Formatting = Formatting.Indented,
          Converters = defaultConverters.ToList()
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
                new DefaultContractResolver(_universe),
                new JsonConverter[] {
                  new Enumeration.JsonConverter()
                }
              );
        } JsonSerializerSettings _componentJsonSerializerSettings;

        /// <summary>
        /// Compiled model serializer from the settings config function
        /// </summary>
        public JsonSerializerSettings ModelJsonSerializerSettings {
          get => _modelJsonSerializerSettings 
            ??= ConfigureModelJsonSerializerSettings(
              new DefaultContractResolver(_universe),
              new JsonConverter[] {
                new Enumeration.JsonConverter()
              }
            );
        } JsonSerializerSettings _modelJsonSerializerSettings;

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
            typeof(ModelComponentsProperty)
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