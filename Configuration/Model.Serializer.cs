using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Model {

    /// <summary>
    /// Logic and Settings Used To Serialize Models
    /// </summary>
    public partial class Serializer {

      /// <summary>
      /// The key used for the field containing the type data for an enum
      /// </summary>
      public const string EnumTypePropertyName = "__type_";

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
          .Create(Options.ComponentJsonSerializerSettings);
      } JsonSerializer _componentJsonSerializer;

      /// <summary>
      /// Compiled model serializer from the settings
      /// </summary>
      public JsonSerializer ModelJsonSerializer {
        get => _modelJsonSerializer ??= JsonSerializer
          .Create(Options.ModelJsonSerializerSettings);
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