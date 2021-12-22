using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Meep.Tech.Data {
  public partial interface IReadableComponentStorage {

    /// <summary>
    /// Used to convert a collection of components to and from a json array
    /// </summary>
    public class ComponentsToJsonCollectionValueConverter : ValueConverter<IReadOnlyDictionary<string, IModel.IComponent>, string> {

      public ComponentsToJsonCollectionValueConverter() :
        base(convertToProviderExpression, convertFromProviderExpression) {
      }

      private static Expression<Func<string, IReadOnlyDictionary<string, IModel.IComponent>>> convertFromProviderExpression = x => FromJsonString(x);
      private static Expression<Func<IReadOnlyDictionary<string, IModel.IComponent>, string>> convertToProviderExpression = x => ToJsonString(x);

      static IReadOnlyDictionary<string, IModel.IComponent> FromJsonString(string componentsJson)
        => JArray.Parse(componentsJson).Select(token =>
          IComponent.FromJson(token as JObject)
        ).ToDictionary(
          component => component.Key,
          component => component
        );

      static string ToJsonString(IReadOnlyDictionary<string, IModel.IComponent> components)
        => JArray.FromObject(components.Select(componentData => componentData.Value.ToJson())).ToString();
    }

    /// <summary>
    /// Used to convert a collection of components to and from a json array
    /// </summary>
    public class ComponentsToJsonConverter : JsonConverter<IReadOnlyDictionary<string, IModel.IComponent>> {

      public override void WriteJson(JsonWriter writer, [AllowNull] IReadOnlyDictionary<string, IModel.IComponent> value, JsonSerializer serializer) {
        JObject[] values = value.Select(componentData => componentData.Value.ToJson()).ToArray();
        writer.WriteStartArray();
        values.ForEach(jObject => serializer.Serialize(writer, jObject));
        writer.WriteEndArray();
      }

      public override IReadOnlyDictionary<string, IModel.IComponent> ReadJson(JsonReader reader, Type objectType, [AllowNull] IReadOnlyDictionary<string, IModel.IComponent> existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if(reader.TokenType != JsonToken.StartArray) {
          throw new ArgumentException($"Components Field for ECSBAM Models requires an array Jtoken to deserialize");
        }
        JArray components = serializer.Deserialize<JArray>(reader);
        
        return components.Select(token =>
          IComponent.FromJson(token as JObject)
        ).ToDictionary(
          component => component.Key,
          component => component
        );
      }
    } 

  }
}
