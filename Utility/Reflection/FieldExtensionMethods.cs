using PropertyAccess;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meep.Tech.Data.Reflection {
  public static class FieldExtensionMethods {
    static readonly Dictionary<int, IPropertyReadAccess> _getterCache
      = new();
    static readonly Dictionary<int, IClassPropertyWriteAccess> _setterCache
      = new();

    /// <summary>
    /// Try to get an attribute
    /// </summary>
    public static bool TryToGetAttribute<TAttribute>(this PropertyInfo property, out TAttribute attribute)
      where TAttribute : Attribute
        => (attribute = property.GetCustomAttribute<TAttribute>()) != null;

    /// <summary>
    /// Faster get value for properties.
    /// </summary>
    public static object Get(this PropertyInfo property, object forObject) {
      // build the key efficiently:
      int methodKey = HashCode.Combine(forObject.GetType().FullName, property.Name);

      // check if it's cached:
      if (_getterCache.TryGetValue(methodKey, out IPropertyReadAccess propertyAccess) && propertyAccess != null) {
        return propertyAccess.GetValue(forObject);
      }

      // Build a property accessor if it's not:
      propertyAccess
        = _getterCache[methodKey]
        = property.DeclaringType.IsValueType
           ? PropertyAccessFactory.CreateForValue(property)
           : PropertyAccessFactory.CreateForClass(property);

      return propertyAccess == null
        ? throw new Exception($"Could not create getter for {property.Name} on {property.DeclaringType.FullName}")
        : propertyAccess.GetValue(forObject);
    }

    /// <summary>
    /// Faster set value for properties.
    /// </summary>
    public static void Set(this PropertyInfo property, object forObject, object value) {
      // build the key efficiently:
      int methodKey = HashCode.Combine(forObject.GetType().FullName, property.Name);

      // check if it's cached:
      if (_setterCache.TryGetValue(methodKey, out IClassPropertyWriteAccess propertyAccess) && propertyAccess != null) {
        propertyAccess.SetValue(forObject, value);
      }

      // Build a property accessor if it's not:
      propertyAccess
        = _setterCache[methodKey]
        = PropertyAccessFactory.CreateForClass(property);

      propertyAccess.SetValue(forObject, value);
    }
  }
}