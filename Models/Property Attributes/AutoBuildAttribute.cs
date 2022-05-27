using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.Data.Configuration {

  /// <summary>
  /// An attribute signifying that this field should be auto incluided in the builder constructor for this model.
  /// <para>Works with DefaultAttribute</para>
  /// </summary>

  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class AutoBuildAttribute : Attribute {

    /// <summary>
    /// Gets a default value for an auto-built field on a model given the model being built and the builder.
    /// </summary>
    /// <returns>The default value if the parameter isn't supplied by the builder.</returns>
    public delegate object DefaultValueGetter(IBuilder builder, IModel model);

    /// <summary>
    /// Validates the final value.
    /// </summary>
    /// <param name="validationError">(optional returnable exception)</param>
    public delegate bool ValueValidator(object value, IBuilder builder, IModel model, out string message, out Exception validationError);

    /// <summary>
    /// If this field must not be null before being returned by the auto build step.
    /// </summary>
    public bool NotNull {
      get;
      init;
    } = false;

    /// <summary>
    /// If this field's parameter must be provided to the builder
    /// </summary>
    public bool IsRequiredAsAParameter {
      get;
      init;
    } = false;

    /// <summary>
    /// Optional override name for the parameter expected by the builder to build this property with.
    /// Defaults to the name of the property this is attached to.
    /// </summary>
    public string ParameterName {
      get;
      init;
    }

    /// <summary>
    /// The property on the archetype that will be used as a default if no value is provided to the builder.
    /// This defaults to: "Default" + ParameterName.
    /// DefaultAttribute will override this default behavior.
    /// </summary>
    public string DefaultArchetypePropertyName {
      get;
      init;
    }

    /// <summary>
    /// The name of a DefaultValueGetter delegate field that can be used to set the default value of this property instead of the DefaultArchetypePropertyName
    /// Defaults to null and uses DefaultArchetypePropertyName instead.
    /// DefaultAttribute will override this default behavior.
    /// </summary>
    public string DefaultValueGetterDelegateName {
      get;
      init;
    }

    /// <summary>
    /// The name of the optional value validator.
    /// This can be a field, property, or funciton that matches the delegate ValueValidator on this same model.
    /// </summary>
    public string ValueValidatorName {
      get;
      init;
    }

    /// <summary>
    /// The build order. Defaults to 0.
    /// </summary>
    public int Order {
      get;
      init;
    } = 0;

    /// <summary>
    /// Mark a field for auto-inclusion in the default builder ctor
    /// </summary>
    public AutoBuildAttribute() { }

    internal static IEnumerable<Func<IModel, IBuilder, IModel>> _generateAutoBuilderSteps(System.Type modelType)
      => modelType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .Where(p => Attribute.IsDefined(p, typeof(AutoBuildAttribute), true))
        .Select(p => {
          AutoBuildAttribute attributeData = p.GetCustomAttributes(typeof(AutoBuildAttribute), true)
            .First() as AutoBuildAttribute;

          return (order: attributeData.Order, func: new Func<IModel, IBuilder, IModel>((IModel m, IBuilder b) => {
            System.Reflection.MethodInfo setter = p.GetSetMethod(true) 
              ?? throw new AccessViolationException($"Can not find a setter for property: {p.Name}, on model: {p.DeclaringType.FullName}, for auto-builder property attribute setup");
            
            object value;
            if(attributeData.IsRequiredAsAParameter) {
              object defaultValue = _getDefaultValue(m, b, p, attributeData);
              value = _getValueFromBuilder(b, p, attributeData, defaultValue);
            }
            else {
              value = _getRequiredValueFromBuilder(b, p, attributeData);
            }

            if(attributeData.ValueValidatorName is not null) {
              // TODO: cache this
              ValueValidator validator = _generateValidator(attributeData.ValueValidatorName, m, p);
              if(!validator.Invoke(value, b, m, out string message, out Exception exception)) {
                throw new ArgumentException($"Invalid Value: {value}, tried to set to parameter/property: {attributeData.ParameterName ?? p.Name}, via auto builder: {message}", exception);
              }
            }

            if(attributeData.NotNull && value is null) {
              throw new ArgumentNullException(attributeData.ParameterName ?? p.Name);
            }

            // TODO: cache this
            setter.Invoke(m, new object[] { value });
            return m;
          })); }
        ).OrderBy(e => e.order)
        .Select(e => e.func);

    static ValueValidator _generateValidator(string valueValidatorName, IModel model, PropertyInfo property) {
      var @delegate = property.DeclaringType.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
          .FirstOrDefault(m => m.Name == valueValidatorName
            && (
              (m is PropertyInfo p)
                && p.PropertyType == typeof(AutoBuildAttribute.ValueValidator)
              || (m is FieldInfo f)
                && f.FieldType == typeof(AutoBuildAttribute.ValueValidator)
              || (m is MethodInfo l)
                && (l.ReturnType == typeof(bool))
                && (l.GetParameters().Select(p => p.ParameterType).SequenceEqual(new System.Type[] {
                  typeof(object),
                  typeof(IBuilder),
                  typeof(IModel),
                  typeof(string),
                  typeof(Exception)
                }) || l.GetParameters().Select(p => p.ParameterType).SequenceEqual(new System.Type[] {
                  property.PropertyType,
                  typeof(IBuilder),
                  typeof(IModel),
                  typeof(string),
                  typeof(Exception)
                }))
            ));

      if(@delegate is MethodInfo method) {
        return new ValueValidator((object v, IBuilder b, IModel m, out string t, out Exception x) => {
          object[] parameters = new object[] { v, b, b, null, null };
          if((bool)method.Invoke(b, parameters)) {
            t = parameters[3] as string;
            x = parameters[4] as Exception;

            return true;
          }
          else {
            t = parameters[3] as string;
            x = parameters[4] as Exception;

            return false;
          }
        });
      }
      else {
        ValueValidator defaultValidatorDelegate = (@delegate is PropertyInfo p
            ? p.GetValue(model)
            : @delegate is FieldInfo f
              ? f.GetValue(model)
              : throw new Exception($"Unknown member type for default builder attribute default value")
          ) as AutoBuildAttribute.ValueValidator;

        return defaultValidatorDelegate;
      }
    }

    static object _getDefaultValue(IModel model, IBuilder builder, PropertyInfo property, AutoBuildAttribute attributeData) {
      object defaultValue = null;
      /// get via default attribute box value.
      // TODO: instead of an overall override, this should be able to be deferred then used if the cached functions provide null in the end.
      var defaultValueAttributeData = property.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault() as DefaultValueAttribute;
      if(defaultValueAttributeData is not null) {
        return defaultValueAttributeData.Value;
      }

      /// get via getter on the model
      if(attributeData.DefaultValueGetterDelegateName is not null) {
        var @delegate = property.DeclaringType.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
          .FirstOrDefault(m => m.Name == attributeData.DefaultValueGetterDelegateName
            && (
              (m is PropertyInfo p) 
                && p.PropertyType == typeof(AutoBuildAttribute.DefaultValueGetter)
              || (m is FieldInfo f) 
                && f.FieldType == typeof(AutoBuildAttribute.DefaultValueGetter)
              || (m is MethodInfo l) 
                && (l.ReturnType == typeof(object) || l.ReturnType == property.PropertyType) 
                && l.GetParameters().Select(p => p.GetType()).SequenceEqual(new System.Type[] {
                  typeof(IBuilder),
                  typeof(IModel)
                }) 
            ));

        if(@delegate is null) {
          throw new MissingMemberException($"Could not find a property or field named {attributeData.DefaultValueGetterDelegateName}, on model type {property.DeclaringType}, that is a delegate of type {typeof(AutoBuildAttribute.DefaultValueGetter).FullName}, or a function with the same name and the return type {typeof(IModel).FullName}, and parameters: ({nameof(IBuilder)}, {nameof(IModel)})");
        }

        if(@delegate is MethodInfo method) {
          // TODO: cache this
          return method.Invoke(model, new object[] { model, builder });
        } else {
          DefaultValueGetter defaultGetterDelegate = (@delegate is PropertyInfo p
              ? p.GetValue(model)
              : @delegate is FieldInfo f
                ? f.GetValue(model)
                : throw new Exception($"Unknown member type for default builder attribute default value")
            ) as AutoBuildAttribute.DefaultValueGetter;

          // TODO: cache this
          return defaultGetterDelegate.Invoke(builder, model);
        }
      }

      /// get via name on the archetype
      if(attributeData.DefaultArchetypePropertyName is not null) {
        var archetypeDefaultProvider = builder.Archetype.GetType().GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
          .FirstOrDefault(m => m.Name == attributeData.DefaultArchetypePropertyName
            && (
              (m is PropertyInfo p) && p.PropertyType == property.PropertyType
              || (m is FieldInfo f) && f.FieldType == property.PropertyType
              || (m is MethodInfo l) 
                && (l.ReturnType == typeof(object) || l.ReturnType == property.PropertyType) 
                && l.GetParameters().Select(p => p.GetType()).SequenceEqual(new System.Type[] {
                  typeof(IBuilder),
                  typeof(IModel)
                }) 
            ));

        if(archetypeDefaultProvider is null) {
          throw new MissingMemberException($"Could not find a property or field named {attributeData.DefaultArchetypePropertyName}, on model type {property.DeclaringType}, that is of type {property.PropertyType.FullName}, or a function with the same name and the return type, with and parameters: ({nameof(IBuilder)}, {nameof(IModel)})");
        }

        if(archetypeDefaultProvider is MethodInfo method) {
          // TODO: cache this
          return method.Invoke(builder.Archetype, new object[] { model, builder });
        } else {
          DefaultValueGetter defaultGetterDelegate = (archetypeDefaultProvider is PropertyInfo p
              ? p.GetValue(builder.Archetype)
              : archetypeDefaultProvider is FieldInfo f
                ? f.GetValue(builder.Archetype)
                : throw new Exception($"Unknown member type for default builder attribute default value")
            ) as AutoBuildAttribute.DefaultValueGetter;

          // TODO: cache this
          return defaultGetterDelegate.Invoke(builder, model);
        }
      }

      /// try to find the standard default property:
      PropertyInfo archetypeDefaultProperty = builder.Archetype.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        .FirstOrDefault(p => 
          ((p.Name == "Default" + (attributeData.ParameterName ?? property.Name))
            || (p.Name.Trim('_').ToLower() == ("Default" + (attributeData.ParameterName ?? property.Name)).ToLower()))
          && p.PropertyType == property.PropertyType
        );

      if(archetypeDefaultProperty is not null) {
        // TODO: cache this
        return archetypeDefaultProperty.GetValue(builder.Archetype);
      }

      return defaultValue;
    }

    static object _getValueFromBuilder(IBuilder builder, PropertyInfo property, AutoBuildAttribute attributeData, object defaultValue) {
      var getter = typeof(BuilderExtensions).GetMethod(nameof(BuilderExtensions.GetParam));
      getter = getter.MakeGenericMethod(property.PropertyType);

      // TODO: cache this
      return getter.Invoke(builder, new[] { attributeData.ParameterName ?? property.Name, defaultValue });
    }

    static object _getRequiredValueFromBuilder(IBuilder builder, PropertyInfo property, AutoBuildAttribute attributeData) {
      var getter = typeof(BuilderExtensions).GetMethod(nameof(BuilderExtensions.GetAndValidateParamAs));
      getter = getter.MakeGenericMethod(property.PropertyType);

      // TODO: cache this
      return getter.Invoke(builder, new[] { attributeData.ParameterName ?? property.Name });
    }
  }
}
