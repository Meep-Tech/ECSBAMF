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
        /// Helper function to set the default json serializer settings for models.
        /// </summary>
        public Func<DefaultContractResolver, IEnumerable<Newtonsoft.Json.JsonConverter>, JsonSerializerSettings> ConfigureJsonSerializerSettings {
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
        /// Compiled model serializer from the settings config function
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings {
          get => _modelJsonSerializerSettings 
            ??= ConfigureJsonSerializerSettings(
              new DefaultContractResolver(_universe),
              new Newtonsoft.Json.JsonConverter[] {
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