using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  public partial class Model {
    public abstract partial class Builder {

      /// <summary>
      /// A static parameter constant for a model builder
      /// </summary>
      public class Param : Enumeration<Param> {

        /// <summary>
        /// A Univerally Unique Key for this param.
        /// </summary>
        public string Key {
          get => _castKey ??= ExternalId as string;
        } string _castKey;

        /// <summary>
        /// If this param has a pre-set default.
        /// </summary>
        public bool HasDefaultValue {
          get;
        }

        /// <summary>
        /// An optional pre-set type to constrain the value to
        /// </summary>
        public Type ValueType {
          get;
        }

        /// <summary>
        /// A base default value for this param.
        /// </summary>
        public object DefaultValue {
          get;
        }

        public Param(string name) 
          : base(name) { }

        public Param(string name, System.Type valueType) 
          : this(name) {
          ValueType = valueType; 
        }

        public Param(string name, object defaultValue, System.Type valueType = null) 
          : this(name, valueType) {
          DefaultValue = defaultValue;
          HasDefaultValue = true;
        }

        /// <summary>
        /// Exception for a missing required parameter
        /// </summary>
        public class MissingException : MissingMemberException {
          public MissingException(string message) : base(message) { }
        }

        /// <summary>
        /// Exception for a param of the wrong type
        /// </summary>
        public class MissmatchException : Exception {
          public MissmatchException(string message) : base(message) { }
        }

      }
    }
  }

  public partial class Model<TModelBase>
    where TModelBase : IModel<TModelBase> 
  {
    public new partial class Builder {

      /// <summary>
      /// A parameter constant for a model builder
      /// </summary>
      public new class Param : Model.Builder.Param {

        /// <summary>
        /// All params registered for this type of model builder.
        /// </summary>
        public new static IEnumerable<Param> All
          => _all; static List<Param> _all
          = new List<Param>();

        /// <summary>
        /// Make a new super basic param.
        /// </summary>
        public Param(string name)
          : base(name) {
          _all.Add(this);
        }

        /// <summary>
        /// Make a param clamped to a specific type
        /// </summary>
        protected Param(string name, System.Type valueType)
          : base(name, valueType) {
          _all.Add(this);
        }

        /// <summary>
        /// Make a param with a default value.
        /// </summary>
        protected Param(string name, object defaultValue, System.Type valueType = null)
          : base(name, defaultValue, valueType) {
          _all.Add(this);
        }
      }
    }
  }
}