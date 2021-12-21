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

      public const string ComponentKeyPropertyName = "__key_";

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
      internal Serializer(Settings options, Universe universe) {
        Options = options ?? new Settings();
        Options._universe = universe;
      }
    }
  }
}