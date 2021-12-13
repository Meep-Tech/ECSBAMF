using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data.Configuration {
  public partial class Loader {

    public partial class Settings {

      /// <summary>
      /// The name to configure for the current universe.
      /// This will be used as it's unique key in the db
      /// </summary>
      public string UniverseName {
        get;
        set;
      }

      /// <summary>
      /// Prevents a type that inherits from Archetype<,> or IModel<> from being built as an archetype during initial loading. this is NOT inherited.
      /// </summary>
      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
      public class DoNotBuildInInitialLoadAttribute
        : Attribute {
      }

      /// <summary>
      /// Prevents a type that inherits from Archetype<,> or IModel<> and it's inherited types from being built into an archetype during initial loading.
      /// </summary>
      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
      public class DoNotBuildThisOrChildrenInInitialLoadAttribute
        : DoNotBuildInInitialLoadAttribute {
      }
    }
  }
}
