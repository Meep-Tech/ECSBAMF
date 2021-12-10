using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Meep.Tech.Data {

  public partial class Model {

    /// <summary>
    /// A Component for an Model. Contains datas. Logic should usually be kept to Archetypes
    /// </summary>
    public partial interface IComponent : Data.IComponent {

      /// <summary>
      /// Prevent serialization of the type:
      /// </summary>
      public class ShouldSerializeTypeInComponentsContractResolver : DefaultContractResolver {

        /// <summary>
        /// Instance
        /// </summary>
        public static readonly ShouldSerializeTypeInComponentsContractResolver Instance
      = new ShouldSerializeTypeInComponentsContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
          JsonProperty property = base.CreateProperty(member, memberSerialization);

          if((property.PropertyName == "type" || property.PropertyName == "<type>k__BackingField")) {
            property.ShouldSerialize = instance => false;
          }

          return property;
        }
      }
    }
  }
}