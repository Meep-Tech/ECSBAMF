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
    }
  }
}
