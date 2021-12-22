using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Model {
    public partial class Serializer {

      /// <summary>
      /// The key used for the field containing the data for the type of component
      /// </summary>
      public const string ComponentKeyPropertyName = "__key_";

      /// <summary>
      /// The key used for the field containing the value collection for a component if it serializes to a colleciton by default
      /// </summary>
      public const string ComponentValueCollectionPropertyName = "__values_";

      /// <summary>
      /// The serializer options
      /// </summary>
      public Settings Options {
        get;
      }

      /// <summary>
      /// Compiled component serializer from the settings
      /// </summary>
      public JsonSerializer ComponentJsonSerializer {
        get => _componentJsonSerializer ??= JsonSerializer
          .Create(Options.ConfigureComponentJsonSerializerSettings(
            new DefaultContractResolver(Options._universe)));
      } JsonSerializer _componentJsonSerializer;

      /// <summary>
      /// Compiled model serializer from the settings
      /// </summary>
      public JsonSerializer ModelJsonSerializer {
        get => _modelJsonSerializer ??= JsonSerializer
          .Create(Options.ConfigureModelJsonSerializerSettings(
            new DefaultContractResolver(Options._universe)));
      } JsonSerializer _modelJsonSerializer;

      /// <summary>
      /// Make a new serializer for a universe
      /// </summary>
      internal Serializer(Settings options, Universe universe) {
        Options = options ?? new Settings();
        Options._universe = universe;
      }
    }
  }
}