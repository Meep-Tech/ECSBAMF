using Meep.Tech.Data.Reflection;
using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base interface for builders
  /// </summary>
  public interface IBuilder {

    /// <summary>
    /// The universe this is being built in
    /// </summary>
    Universe Universe {
      get;
    }
    
    /// <summary>
    /// The archetype that initialize the building and made the builder
    /// </summary>
    Archetype Archetype {
      get;
    }

    /// <summary>
    /// The parent mode, if this builder was passed down from one. Null if there is no parent
    /// </summary>
    IModel Parent {
      get;
      internal set;
    }

    /// <summary>
    /// The parameters contained in this builder as a list.
    /// </summary>
    IEnumerable<(string name, object value)> Parameters {
      get;
    }

    /// <summary>
    /// Do something for each param in a builder
    /// </summary>
    void ForEachParam(Action<(string key, object value)> @do);

    /// <summary>
    /// Return a copy of the builder with a new value appended.
    /// </summary>
    IBuilder Append(string key, object value);

    internal bool _tryToGetRawValue(string key, out object value);

    internal void _add(string key, object value);
  }

  /// <summary>
  /// The base interface for builders
  /// </summary>
  public interface IBuilder<TModelBase>
    : IBuilder
    where TModelBase : IModel<TModelBase> 
  {

    /// <summary>
    /// used by a builder to initialize it's model.
    /// Uses IFactory.ModelConstructor(builder): by default.
    /// </summary>
    Func<IModel<TModelBase>.Builder, TModelBase> InitializeModel
      => builder => (TModelBase)((IFactory)builder.Archetype).ModelConstructor(builder);

    /// <summary>
    /// Used by a builder to configure it's model
    /// </summary>
    Func<IModel<TModelBase>.Builder, TModelBase, TModelBase> ConfigureModel
      => null;

    /// <summary>
    /// Used by a builder to finalize it's model
    /// </summary>
    Func<IModel<TModelBase>.Builder, TModelBase, TModelBase> FinalizeModel
      => null;

    /// <summary>
    /// Execute a builder
    /// </summary>
    TModelBase Build();
  }

  public static class BuilderExtensions {

    #region Access Via Param

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static T GetViaParam<T>(this IBuilder builder, IModel.Builder.Param toFetch, T defaultValue = default) {
      if (toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toFetch.Key}, is clamped to the type: {toFetch.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if (builder._tryToGetRawValue(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if (value is T typedValue) {
          return typedValue;
        }

        // if the provided value is null, and this is nullable, return the default value
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if (canBeNull && value == null) {
          return toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          return (T)value.CastTo(toFetch.ValueType);
        }
        catch (Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {toFetch.ValueType.FullName}", e);
        }
      }

      return toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
    }

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static bool TryToGetViaParam<T>(this IBuilder builder, IModel.Builder.Param toFetch, out T result, T defaultValue = default) {
      if (toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toFetch.Key}, is clamped to the type: {toFetch.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if (builder._tryToGetRawValue(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if (value is T typedValue) {
          result = typedValue;

          return true;
        }

        // if the provided value is null, and this is nullable, return the default value
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if (canBeNull && value == null) {
          result = toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
          return false;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          result = (T)value.CastTo(toFetch.ValueType);
          return true;
        }
        catch (Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {toFetch.ValueType.FullName}.", e);
        }
      }

      result = toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
      return false;
    }

    /// <summary>
    /// Fetch a param from a collection. The param cannot be left out, and no defaults will be replaced.
    /// </summary>
    public static T GetAndValidateViaParamAs<T>(this IBuilder builder, IModel.Builder.Param toFetch) {
      if (toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}, but the provided Param object expects a value of Type: {toFetch.ValueType.FullName}.");
      }
      if (builder._tryToGetRawValue(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if (value is T typedValue) {
          return typedValue;
        }

        // if the provided value is null, and this is nullable, return the provided null
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if (canBeNull && value == null) {
          return default;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          return (T)value.CastTo(toFetch.ValueType);
        }
        catch (Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param: {toFetch.ExternalId}, as Type: {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {toFetch.ValueType}.", e);
        }
      }

      throw new IModel.Builder.Param.MissingException($"Tried to construct a model without the required param: {toFetch} of type {typeof(T).FullName} being provided. If this is a test, try adding a default value for the empty model for this required param to the Archetype's DefaultTestParams field.");
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetViaParam<T>(this IBuilder builder, IModel.Builder.Param toSet, T value = default) {
      if (builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }
      if (toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }

      builder._add(toSet.Key, value);
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetDefaultValueViaParam<T>(this IBuilder builder, IModel.Builder.Param toSet, T defaultValue) {
      if (builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }
      if (toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if (!builder._tryToGetRawValue(toSet.Key, out _)) {
        builder._add(toSet.Key, defaultValue);
      }
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetViaParamToDefault<T>(this IBuilder builder, IModel.Builder.Param toSet) {
      if (builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }
      if (toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if (!builder._tryToGetRawValue(toSet.Key, out _)) {
        builder._add(toSet.Key, toSet.HasDefaultValue
          ? toSet.DefaultValue
          : throw new IModel.Builder.Param.MissingException($"Param {toSet.Key} tried to set using a default value, but it does not have one set up."));
      }
    }

    #endregion

    #region Access Via String

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static T GetParam<T>(this IBuilder builder, string paramKey, T defaultValue = default) {
      if (builder is not null) {
        Type valueType = typeof(T);
        if (builder._tryToGetRawValue(paramKey, out object value)) {
          // if the value is of the requested type, return it
          if (value is T typedValue) {
            return typedValue;
          }

          // if the provided value is null, and this is nullable, return the default value
          bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
          if (canBeNull && value == null) {
            return defaultValue;
          }

          // See if this can be cast to the valuetype of the param, and return it if it can.
          try {
            return (T)value.CastTo(valueType);
          } catch (Exception e) {
            throw new IModel.Builder.Param.MissmatchException($"Tried to get param: {paramKey}, as Type: {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {valueType.FullName}.", e);
          }
        }
      }

      return defaultValue;
    }

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static bool TryToGetParam<T>(this IBuilder builder, string paramKey, out T result, T defaultValue = default) {
      if (builder is not null) {
        Type valueType = typeof(T);
        if (builder._tryToGetRawValue(paramKey, out object value)) {
          // if the value is of the requested type, return it
          if (value is T typedValue) {
            result = typedValue;
            return true;
          }

          // if the provided value is null, and this is nullable, return the default value
          bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
          if (canBeNull && value == null) {
            result = defaultValue;
            return false;
          }

          // See if this can be cast to the valuetype of the param, and return it if it can.
          try {
            result = (T)value.CastTo(valueType);
            return true;
          } catch (Exception e) {
            throw new IModel.Builder.Param.MissmatchException($"Tried to get param: {paramKey}, as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {valueType.FullName}", e);
          }
        }
      }

      result = defaultValue;
      return false;
    }

    /// <summary>
    /// Check if this has the given param
    /// </summary>
    public static bool HasParam(this IBuilder builder, string paramName)
      => builder is not null && builder._tryToGetRawValue(paramName, out _);

    /// <summary>
    /// Check if this has the given param
    /// </summary>
    public static bool HasParam(this IBuilder builder, string paramName, out System.Type paramType) { 
      if (builder._tryToGetRawValue(paramName, out var parameter)) {
        paramType = parameter.GetType();
        return true;
      }

      paramType = null;
      return false;
    }

    /// <summary>
    /// Fetch a param from a collection. The param cannot be left out, and no defaults will be replaced.
    /// </summary>
    public static T GetAndValidateParamAs<T>(this IBuilder builder, string paramKey) {
      if (builder is not null) {
        Type valueType = typeof(T);
        if (builder._tryToGetRawValue(paramKey, out object value)) {
          // if the value is of the requested type, return it
          if (value is T typedValue) {
            return typedValue;
          }

          // if the provided value is null, and this is nullable, return the provided null
          bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
          if (canBeNull && value == null) {
            return default;
          }

          // See if this can be cast to the valuetype of the param, and return it if it can.
          try {
            return (T)value.CastTo(valueType);
          } catch (Exception e) {
            throw new IModel.Builder.Param.MissmatchException($"Tried to get param: {paramKey}, as type {typeof(T).FullName}. The provided invalid value has Type {value?.GetType().FullName ?? "null"}, but should be of Type: {valueType.FullName}.", e);
          }
        }
      }

      throw new IModel.Builder.Param.MissingException($"Tried to construct a model without the required param: {paramKey} of type {typeof(T).FullName} being provided. If this is a test, try adding a default value for the empty model for this required param to the Archetype's {nameof(Archetype.DefaultTestParams)} field.");
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetDefaultParamValue<T>(this IBuilder builder, string parameter, T defaultValue = default) {
      if(builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }
      if (builder is not null) {
        if (!builder._tryToGetRawValue(parameter, out _)) {
          builder._add(parameter, defaultValue);
        }
      }
    }

    /// <summary>
    /// set a value
    /// </summary>
    public static void SetParam<T>(this IBuilder builder, string parameter, T defaultValue = default) {
      if (builder is not null) {
        if (builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
          throw new AccessViolationException($"Cannot change params on an immutable builder");
        }

        builder._add(parameter, defaultValue);
      }
    }

    #endregion
  }
}