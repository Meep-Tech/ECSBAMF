using System;

namespace Meep.Tech.Data.Configuration {
  public partial class Loader {

    public partial class Settings {
      /// <summary>
      // Used as shorthand for an archetype that produces a different model via the Model constructor
      /// This will just set the model constructor of the archetype to the basic activator for the parameterless ctor of TNewBaseModel, or the declaring type of the current type.     
      /// TODO: test if we can get only the newest "branch" attribute applied somehow :
      /// /// If you want to give a default "behavior" in the base class and override it in some derived classes you have to check all the attributes returned by GetCustomAttributes() to use only the most derived one (the first in the list).
      /// </summary>
      [AttributeUsage(AttributeTargets.Class, Inherited = true)]
      public class BranchAttribute
        : Attribute {

        /// <summary>
        /// The new base model this archetype branches for
        /// </summary>
        public Type NewBaseModelType {
          get;
          internal set;
        }

        public BranchAttribute(Type newBaseModelType = null) {
          NewBaseModelType = newBaseModelType;
        }
      }
    }
  }
}
