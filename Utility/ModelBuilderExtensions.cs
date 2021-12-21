using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace Meep.Tech.Data {
  public static class ModelBuilderExtensions {

    static readonly ValueConverter _componentConverter 
      = new IReadableComponentStorage.ComponentsToJsonCollectionValueConverter();

    /// <summary>
    /// Used to set up Ecsbam settings needed for general models in your custom DbContext class.
    /// </summary>
    public static ModelBuilder SetUpEcsbamModels(this ModelBuilder modelBuilder, Universe _universe) {
      modelBuilder.AddModelTypes(_universe);
      modelBuilder.UseCustomValueConverters();

      return modelBuilder;
    }

    public static ModelBuilder AddModelTypes(this ModelBuilder modelBuilder, Universe universe) {
      foreach((System.Type modelType, var config) in universe.ModelSerializer.Options.TypesToMapToDbContext) {
        var builder = modelBuilder.Entity(modelType);
        // Archetype based builder types use Table Per Hirearchy pattern, and use the Archetype field as a discriminator
        if(modelType.IsAssignableToGeneric(typeof(Model<,>))) {
          builder.HasDiscriminator(nameof(Archetype), typeof(Archetype));
        }

        // unique have their id field as key
        if(typeof(IUnique).IsAssignableFrom(modelType)) {
          modelBuilder.Entity(modelType.FullName).Property(typeof(string), "Id")
            .IsRequired().HasAnnotation("Key", 0);
        } // if a user wants to set a custom key, they need to apply this interface. 
        else if (!typeof(IUniqueWithCustomKeyColumn).IsAssignableFrom(modelType)
          // only apply HasNoKey to base model types
          && modelType.GetModelBaseType().Equals(modelType)
        ) {
          builder.HasNoKey();
        }

        // custom config
        config?.Invoke(builder);
      }

      return modelBuilder;
    }

    /// <summary>
    /// Can be used in DBContext to set up the custom value converters
    /// </summary>
    public static ModelBuilder UseCustomValueConverters(this ModelBuilder modelBuilder) {
      foreach(var entityType in modelBuilder.Model.GetEntityTypes()) {
        // note that entityType.GetProperties() will throw an exception, so we have to use reflection 
        foreach(var property in entityType.ClrType.GetProperties()) {
          UseCustomConverterAttribute useCustomConverterAttribute 
            = property.GetCustomAttributes(typeof(UseCustomConverterAttribute), true).FirstOrDefault() 
              as UseCustomConverterAttribute;
          if(!(useCustomConverterAttribute is null)) {
            // if we have a cached converter of this type, use that.
            ValueConverter customConverter;
            if(useCustomConverterAttribute is IsArchetypePropertyAttribute) {
              //Type modelBaseType = entityType.ClrType.GetModelBaseType();
              Type converterType = typeof(Archetype.ToKeyStringConverter<>)
                .MakeGenericType(property.PropertyType);
              customConverter = (ValueConverter)Activator.CreateInstance(converterType);
            }
             else if(useCustomConverterAttribute is IsModelComponentsProperty) {
              customConverter = _componentConverter;
            } else {
             customConverter = UseCustomConverterAttribute
                ._cachedCustomConverters[useCustomConverterAttribute.CustomConverterType];
            }
            modelBuilder.Entity(entityType.Name).Property(property.Name)
              .HasConversion(customConverter);
          }
        }
      }

      return modelBuilder;
    }

    /// <summary>
    /// Use a custom value converter for all properties of a type
    /// </summary>
    public static ModelBuilder UseValueConverterForType<T>(this ModelBuilder modelBuilder, ValueConverter converter) {
      return modelBuilder.UseValueConverterForType(typeof(T), converter);
    }

    /// <summary>
    /// Use a custom value converter for all properties of a type
    /// </summary>
    public static ModelBuilder UseValueConverterForType(this ModelBuilder modelBuilder, Type type, ValueConverter converter) {
      foreach(var entityType in modelBuilder.Model.GetEntityTypes()) {
        // note that entityType.GetProperties() will throw an exception, so we have to use reflection 
        var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == type);
        foreach(var property in properties) {
          modelBuilder.Entity(entityType.Name).Property(property.Name)
              .HasConversion(converter);
        }
      }

      return modelBuilder;
    }
  }

}
