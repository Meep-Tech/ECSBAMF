using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.Data {

  /// <summary>
  /// An attribute that adds a value to the DefaultTestParams field of an archetype by default.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueAttribute : Attribute {

    /// <summary>
    /// The value of the attribute
    /// </summary>
    public virtual object Value {
      get => _value;
      init => _value = value;
    } internal object _value;

    /// <summary>
    /// Set the DefaultTestParam value
    /// </summary>
    /// <param name="value"></param>
    public TestValueAttribute(object value = null) {
      Value = value;
    }

    internal static Dictionary<string, object> _generateTestParameters(Archetype factoryType, Type modelType) {
      Dictionary<string, object> @params = factoryType.DefaultTestParams ?? new();
      foreach ((PropertyInfo property, TestValueAttribute attribute) in modelType
        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Select(p => (p, a: p.GetCustomAttribute<TestValueAttribute>(true)))
        .Where(e => e.a is not null)
      ) {
        if (attribute is TestValueIsNewAttribute) {
          attribute._value ??= Activator.CreateInstance(property.PropertyType);
        }
        else if (attribute is TestValueIsEmptyEnumerableAttribute) {
          attribute._value ??= typeof(Enumerable).GetMethod(nameof(Enumerable.Empty), BindingFlags.Static | BindingFlags.Public)
            .MakeGenericMethod(property.PropertyType.GetGenericArguments().First()).Invoke(null, new object[0]);
        }
        else if (attribute is GetTestValueFromMemberAttribute memberAttribute && memberAttribute.Value is null) {
          try {
            System.Type currentModelType = modelType;
            MemberInfo[] members = new MemberInfo[0];
            MemberInfo member;
            while (!members.Any() && typeof(IModel).IsAssignableFrom(currentModelType)) {
              members = currentModelType.GetMember(memberAttribute.MethodName, BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);
              currentModelType = currentModelType.BaseType;
            }

            member = members.First();
            if (member is MethodInfo method) {
              var methodParams = method.GetParameters();
              if (methodParams.Any()) {
                if (methodParams.Length > 1 || !typeof(Archetype).IsAssignableFrom(methodParams.First().ParameterType)) {
                  throw new ArgumentException($"GetTestValueFromMemberAttribute requires a static property, field, or method (with 0 or 1 parameter(s)). If 1 parameter is provided for a method it must be of type Archetype.");
                }
                attribute._value = method.Invoke(null, new object[] {
                    factoryType
                  });
              }
              else {
                attribute._value = method.Invoke(null, new object[0]);
              }

            }
            else if (member is PropertyInfo prop) {
              attribute._value = prop.GetValue(null);
            }
            else if (member is FieldInfo field) {
              attribute._value = field.GetValue(null);
            }
          }
          catch (Exception e) {
            throw new MissingMemberException($"Member {memberAttribute.MethodName} not found, or {memberAttribute.MethodName} is not a static property, field, or method (with 0 or 1 parameter(s)). If 1 parameter is provided for a method it must be of type Archetype.", e);
          }
        }

        AutoBuildAttribute autoBuildData;
        string fieldName = property.Name;
        if ((autoBuildData = property.GetCustomAttribute<AutoBuildAttribute>(true)) != null) {
          fieldName = autoBuildData.ParameterName ?? fieldName;
        }

        @params[fieldName] = attribute.Value;
      }

      return @params;
    }
  }

  /// <summary>
  /// An attribute that activates a default object of the type as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueIsNewAttribute : TestValueAttribute {

    /// <summary>
    /// Set the test value of this field to 'new()';
    /// </summary>
    public TestValueIsNewAttribute() : base() { }
  }

  /// <summary>
  /// An attribute that gets a value from a local member as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class GetTestValueFromMemberAttribute : TestValueAttribute {

    /// <summary>
    /// The name of the method to use to get the value.
    /// </summary>
    public string MethodName { 
      get;
      init;
    }

    /// <summary>
    /// Set the test value of this field to 'new()';
    /// </summary>
    public GetTestValueFromMemberAttribute(string methodName) : base() {
      MethodName = methodName;
    }
  }

  /// <summary>
  /// An attribute that activates an empty enum of the seired type as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueIsEmptyEnumerableAttribute : TestValueAttribute {

    /// <summary>
    /// Set the test value of this field to 'Enumerable.Empty<T>()';
    /// </summary>
    public TestValueIsEmptyEnumerableAttribute() : base() { }
  }
}
