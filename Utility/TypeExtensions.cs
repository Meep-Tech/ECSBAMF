using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Meep.Tech.Data {

  /// <summary>
  /// Shortcuts and caching for casting types for xbam
  /// </summary>
  public static class TypeExtensions {

    /// <summary>
    /// Modify and return something.
    /// </summary>
    public static T Modify<T>(this T @object, Func<T, T> modifier)
      => modifier(@object);

    /// <summary>
    /// Modify and return an object.
    /// </summary>
    public static T Modify<T>(this T @object, Action<T> modifier)
      where T : class {
      modifier(@object);
      return @object;
    }

    #region Inheritance Testing

    /// <summary>
    /// Check if a given type is assignable to a generic type
    /// </summary>
    /// <param name="givenType"></param>
    /// <param name="genericType"></param>
    /// <returns></returns>
    public static bool IsAssignableToGeneric(this Type givenType, Type genericType) {
      var interfaceTypes = givenType.GetInterfaces();

      foreach(var it in interfaceTypes) {
        if(it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
          return true;
      }

      if(givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        return true;

      Type baseType = givenType.BaseType;
      if(baseType == null)
        return false;

      return baseType.IsAssignableToGeneric(genericType);
    }

    /// <summary>
    /// Get the generic arguments from a type this inherits from
    /// </summary>
    public static IEnumerable<Type> GetInheritedGenericTypes(this Type type, Type genericParentType) {
      List<Type> inheritedGenericTypes = new List<Type>();
      foreach(Type intType in type.GetParentTypes()) {
        if(intType.IsGenericType && intType.GetGenericTypeDefinition() == genericParentType) {
          return intType.GetGenericArguments();
        }
      }

      return inheritedGenericTypes;
    }

    /// <summary>
    /// Get all parent types and interfaces 
    /// </summary>
    public static IEnumerable<Type> GetParentTypes(this Type type) {
      // is there any base type?
      if(type == null) {
        yield break;
      }

      // return all implemented or inherited interfaces
      foreach(var i in type.GetInterfaces()) {
        yield return i;
      }

      // return all inherited types
      var currentBaseType = type.BaseType;
      while(currentBaseType != null) {
        yield return currentBaseType;
        currentBaseType = currentBaseType.BaseType;
      }
    }

    /// <summary>
    /// Get the depth of inheritance of a type
    /// </summary>
    public static int GetDepthOfInheritance(this Type type) {
      int index = 0;
      while (type.BaseType != null) {
        index++;
        type = type.BaseType;
      }

      return index;
    }

    #endregion

    #region Casting

    static readonly Dictionary<Tuple<Type, Type>, Func<object, object>> CastCache
    = new Dictionary<Tuple<Type, Type>, Func<object, object>>();

    static Func<object, object> MakeCastDelegate(Type originalType, Type resultingType) {
      var inputObject = Expression.Parameter(typeof(object));
      return Expression.Lambda<Func<object, object>>(
          Expression.Convert(
            Expression.ConvertChecked(
              Expression.Convert(inputObject, originalType),
              resultingType
            ),
            typeof(object)
          ),
          inputObject
        ).Compile();
    }

    static Func<object, object> GetCastDelegate(Type from, Type to) {
      lock(CastCache) {
        var key = new Tuple<Type, Type>(from, to);
        Func<object, object> cast_delegate;
        if(!CastCache.TryGetValue(key, out cast_delegate)) {
          try {
            cast_delegate = MakeCastDelegate(from, to);
          }
          catch(Exception e) {
            throw new NotImplementedException($"No cast found from:\n\t{from.FullName},\nto:\n\t{to.FullName}.\n\n{e}");
          }
          CastCache.Add(key, cast_delegate);
        }
        return cast_delegate;
      }
    }

    /// <summary>
    /// Tries to cast an object to a given type. First time is expensive
    /// </summary>
    public static object CastTo(this object @object, Type type) {
      return GetCastDelegate(@object.GetType(), type).Invoke(@object);
    }

    #endregion

    /// <summary>
    /// Can be used to get any type by it's full name. Searches all assemblies and returns first match.
    /// </summary>
    public static Type GetTypeByFullName(string typeName) {
      Type type = Type.GetType(typeName);
      if (type != null) {
        return type;
      }

      foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
        type = assembly.GetType(typeName);
        if (type != null) {
          return type;
        }
      }

      return null;
    }

    /// <summary>
    /// Get a clean, easier to read type name that's still fully qualified.
    /// </summary>
    public static string ToFullHumanReadableNameString(this Type type, bool withNamespace = true, IEnumerable<Type> genericTypeOverrides = null) {
      if (type.IsGenericParameter) {
        return type.Name;
      }

      if (!type.IsGenericType) {
        return type.FullName;
      }

      System.Text.StringBuilder builder
        = new();

      if (withNamespace) {
        builder.Append(type.Namespace);
        builder.Append(".");
      }

      if (type.DeclaringType != null) {
        builder.Append(ToFullHumanReadableNameString(type.DeclaringType, false, type.GetGenericArguments()));
        builder.Append(".");
      }

      if (!type.Name.Contains("`")) {
        builder.Append(type.Name);
        return builder.ToString();
      } else
        builder.Append(type.Name[..type.Name.IndexOf("`")]);

      builder.Append('<');
      bool first = true;
      foreach (Type genericTypeArgument in genericTypeOverrides ?? type.GetGenericArguments()) {
        if (!first) {
          builder.Append(',');
        }
        builder.Append(genericTypeArgument.ToFullHumanReadableNameString());
        first = false;
      }
      builder.Append('>');

      return builder.ToString();
    }
  }
}
