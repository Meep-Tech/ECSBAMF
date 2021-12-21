using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.Data {

  public partial class Model {
    public partial class Serializer {
      public class DefaultContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver {

        public Universe Universe {
          get;
        }

        IFactory.JsonStringConverter _factoryToStringJsonConverter 
          = new IFactory.JsonStringConverter();

        IReadableComponentStorage.ComponentsToJsonConverter _componentsJsonConverter 
          = new IReadableComponentStorage.ComponentsToJsonConverter();

        public DefaultContractResolver(Universe universe) {
          IgnoreSerializableAttribute = false;
          NamingStrategy = new CamelCaseNamingStrategy() {
            OverrideSpecifiedNames = false
          };
          Universe = universe;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
          var baseProps = base.CreateProperties(type, memberSerialization);
          // remove the universe property if there's one. It gets assinged as part of the archetype or key value
          baseProps.Remove(
            baseProps.FirstOrDefault(prop
              => prop.PropertyName == nameof(Universe).ToLower())
          );
          // Add unique ids if there isn't one already
          if(typeof(IUnique).IsAssignableFrom(type)) {
            if(!baseProps.Any(prop => prop.PropertyName == "id")) {
              PropertyInfo idProp = type.GetInterface(nameof(IUnique))
                .GetProperty(nameof(IUnique.Id));
              if(idProp == null) {
                throw new NotImplementedException($"Types that inherit from IUnique require a serializeable 'id' property. {type.FullName} Does not have one.");
              }
              baseProps.Add(CreateProperty(idProp, memberSerialization));
            }
          }

          return baseProps;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
          JsonProperty baseProperty = base.CreateProperty(member, memberSerialization);
          if(!(member.GetCustomAttribute(typeof(IsArchetypePropertyAttribute)) is null)) {
            // Archetype is always first:
            baseProperty.Order = int.MinValue;
            baseProperty.Converter = _factoryToStringJsonConverter;
            baseProperty.ObjectCreationHandling = ObjectCreationHandling.Replace;
          }

          if(!(member.GetCustomAttribute(typeof(IsModelComponentsProperty)) is null)) {
            baseProperty.Converter = _componentsJsonConverter;
            baseProperty.ObjectCreationHandling = ObjectCreationHandling.Replace;
          }

          /// Any property with a set method is included. Opt out is the default.
          if(Universe.ModelSerializer.Options.PropertiesMustOptOutForJsonSerialization 
            && member is PropertyInfo property 
            && typeof(IModel).IsAssignableFrom(property.DeclaringType)
          ) {
            if(property.SetMethod != null) {
              baseProperty.Writable = true;
            }
          }
          
          return baseProperty;
        }
      }
    }
  }
}