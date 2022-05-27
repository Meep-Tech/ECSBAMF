using System;

namespace Meep.Tech.Data.Configuration {
  public partial class Loader {

    public partial class Settings {
      /// <summary>
      /// Used as shorthand for an archetype that produces a different model via the Model constructor
      /// This will just set the model constructor of the archetype to the basic activator for the parameterless ctor of TNewBaseModel, or the declaring type of the current type.     
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

      /// <summary>
      /// Used as shorthand for a property in a model that can be set by a matching property in an archetype.
      /// TODO: this is not implemented and may require code generation!!!!!
      /// </summary>
      /*[AttributeUsage(AttributeTargets.Property, Inherited = false)]
      public class ArchetypePropertyAttribute
        : Attribute {

        public ArchetypePropertyAttribute() {}
      }*/
    }
  }
}
