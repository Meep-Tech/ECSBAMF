using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using KellermanSoftware.CompareNetObjects;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
        /// The db context used by the serializer
        /// </summary>
        public DbContext DbContext {
          get;
          internal set;
        }

        /// <summary>
        /// Whether or not ECSBAM should set up the models with a db context
        /// </summary>
        public bool TryToSetUpDbContext {
          get;
          set;
        } = true;

        /// <summary>
        /// If this is true, models must have the [Table] attribute to be set up by ecsbam using efcore by default.
        /// </summary>
        public bool ModelsMustOptInToEfCoreUsingAttribute {
          get;
          set;
        } = false;

        /// <summary>
        /// The default entity framework db serializer context
        /// </summary>
        public Func<
          DbContextOptions<Model.Serializer.DbContext>, // general options obj
          Universe,
          Model.Serializer.DbContext // the returned options
        > GetDefaultDbContextForModelSerialization {
          get;
          set;
        } = (options, universe)
          => new Model.Serializer.DbContext(
            options,
            universe
          );

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