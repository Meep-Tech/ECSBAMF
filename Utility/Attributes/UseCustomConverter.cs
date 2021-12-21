using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// Marks a model property field as the Archetype field.
  /// Can also be used for general Archetype [de]serialization with EF core.
  /// </summary>
  public class IsArchetypePropertyAttribute : UseCustomConverterAttribute {
    public IsArchetypePropertyAttribute() 
      : base(null) {}
  }

  /// <summary>
  /// Marks a model property field as the Archetype field.
  /// Can also be used for general Archetype [de]serialization with EF core.
  /// </summary>
  public class IsModelComponentsProperty : UseCustomConverterAttribute {
    public IsModelComponentsProperty() 
      // TODO: this should be a json converter
      : base(typeof(IReadableComponentStorage.ComponentsToJsonCollectionValueConverter)) {}
  }

  /// <summary>
  /// Can be used to set a custom converter for a field on an Ecsbam Model.
  /// You can use the functions in ModelBuilderExtensions to set this up on your DbContext
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class UseCustomConverterAttribute : Attribute {

    internal static Dictionary<Type, ValueConverter> _cachedCustomConverters
      = new Dictionary<Type, ValueConverter>();

    /// <summary>
    /// The custom type converter
    /// </summary>
    public Type CustomConverterType {
      get;
    }

    public UseCustomConverterAttribute(Type customConverterType) {
      // these are handled differently in the logic
      if(this is IsArchetypePropertyAttribute || this is IsModelComponentsProperty) {
        return;
      }

      if(!customConverterType.IsAssignableToGeneric(typeof(ValueConverter<,>))) {
        throw new ArgumentException($"Type of {customConverterType} is not a Converter<,>.");
      }

      CustomConverterType = customConverterType;
      Func<ValueConverter> customConverterCtor 
        = () => Activator.CreateInstance(CustomConverterType) as ValueConverter;

      // Make sure there's a parameterless Ctor for the ValueConverter
      try {
        _cachedCustomConverters[CustomConverterType] = customConverterCtor();
      }
      catch(Exception ex) {
        throw new ArgumentException($"Could not invoke Activator.CreateInstance for parameterless ctor for ValueConverter of type {CustomConverterType}", ex);
      }
    }
  }
}
