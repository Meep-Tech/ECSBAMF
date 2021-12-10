using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;

namespace Meep.Tech.Data {
  public static class ModelBuilderExtensions {

    /// <summary>
    /// Used to set up Ecsbam settings needed for general models in your custom DbContext class.
    /// </summary>
    public static ModelBuilder SetUpEcsbamModels(this ModelBuilder modelBuilder) {
      modelBuilder.UseCustomValueConverters();

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
            = property.GetCustomAttributes(typeof(UseCustomConverterAttribute), true).First() 
              as UseCustomConverterAttribute;
          if(!(useCustomConverterAttribute is null)) {
            // if we have a cached converter of this type, use that.
            ValueConverter customConverter
              = UseCustomConverterAttribute._cachedCustomConverters[useCustomConverterAttribute.CustomConverterType];
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
