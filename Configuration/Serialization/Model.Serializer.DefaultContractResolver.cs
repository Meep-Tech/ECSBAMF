using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Meep.Tech.Data {

  public partial class Model {

    public partial class Serializer {

      /// <summary>
      /// The default contract resolver class used for json serialization by ECSBAM
      /// </summary>
      public class DefaultContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver {
        IFactory.JsonStringConverter _factoryToStringJsonConverter 
          = new IFactory.JsonStringConverter();
        IReadableComponentStorage.ComponentsToJsonConverter _componentsJsonConverter 
          = new IReadableComponentStorage.ComponentsToJsonConverter();

        /// <summary>
        /// the universe this resolver is for
        /// </summary>
        public Universe Universe {
          get;
        }

        /// <summary>
        /// the universe this resolver is for
        /// </summary>
        public IContractResolver _baseResolver {
          get;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DefaultContractResolver(Universe universe) {
          IgnoreSerializableAttribute = false;
          NamingStrategy = new CamelCaseNamingStrategy() {
            OverrideSpecifiedNames = false
          };
          Universe = universe;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
          JsonProperty baseProperty = base.CreateProperty(member, memberSerialization);
          if(!(member.GetCustomAttribute(typeof(ArchetypePropertyAttribute)) is null)) {
            // Archetype is always first:
            baseProperty.Order = int.MinValue;
            baseProperty.Converter = _factoryToStringJsonConverter;
            baseProperty.ObjectCreationHandling = ObjectCreationHandling.Replace;
          }

          if(!(member.GetCustomAttribute(typeof(ModelComponentsProperty)) is null)) {
            baseProperty.Order = int.MaxValue;
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