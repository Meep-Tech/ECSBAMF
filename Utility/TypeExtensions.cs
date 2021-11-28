using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Meep.Tech.Data {

  /// <summary>
  /// Shortcuts and caching for casting types
  /// </summary>
  public static class TypeExtensions {

    #region Inheritance Testing
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
  }
}
