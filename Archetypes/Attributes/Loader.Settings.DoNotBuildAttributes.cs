using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data.Configuration {
  public partial class Loader {

    public partial class Settings {

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
