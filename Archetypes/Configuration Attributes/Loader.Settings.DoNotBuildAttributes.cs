using System;

namespace Meep.Tech.Data.Configuration {
  public partial class Loader {

    public partial class Settings {

      /// <summary>
      /// Can be used for Enumerations to tell the loader to try to pre-initialize all static enum values in this class.
      /// </summary>
      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
      public class BuildAllDeclaredEnumValuesOnInitialLoadAttribute
        : Attribute {
      }

      /// <summary>
      /// Prevents a type that inherits from Archetype or IModel from being built as an archetype during initial loading. this is NOT inherited.
      /// </summary>
      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
      public class DoNotBuildInInitialLoadAttribute
        : Attribute {
      }

      /// <summary>
      /// Prevents a type that inherits from Archetype or IModel and it's inherited types from being built into an archetype during initial loading.
      /// </summary>
      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
      public class DoNotBuildThisOrChildrenInInitialLoadAttribute
        : DoNotBuildInInitialLoadAttribute {
      }
    }
  }
}
