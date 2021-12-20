using System;
using static Meep.Tech.Data.IModel.Builder;

namespace Meep.Tech.Data {

  public interface IBuilder {

    Universe Universe {
      get;
    }
    
    Archetype Archetype {
      get;
    }

    internal bool _tryToGetRawValue(string key, out object value);

    internal void _add(string key, object value);

    void ForEachParam(Action<(string key, object value)> @do);
  }

  public interface IBuilder<TModelBase>
    : IBuilder
    where TModelBase : IModel<TModelBase> 
  {

    Func<IModel<TModelBase>.Builder, TModelBase> InitializeModel
      => builder => (TModelBase)((IFactory)builder.Archetype).ModelConstructor(builder);

    Func<IModel<TModelBase>.Builder, TModelBase, TModelBase> ConfigureModel
      => null;

    Func<IModel<TModelBase>.Builder, TModelBase, TModelBase> FinalizeModel
      => null;

    TModelBase Build();
  }

  public static class BuilderExtensions {

    #region Access Via Param

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static T GetParam<T>(this IBuilder builder, IModel.Builder.Param toFetch, T defaultValue = default) {
      if(toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toFetch.Key}, is clamped to the type: {toFetch.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if(builder._tryToGetRawValue(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if(value is T typedValue) {
          return typedValue;
        }

        // if the provided value is null, and this is nullable, return the default value
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if(canBeNull && value == null) {
          return toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          return (T)value.CastTo(toFetch.ValueType);
        }
        catch(Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. But param has type {value?.GetType().FullName ?? "null"}, and should be type {toFetch.ValueType}.\n{e}");
        }
      }

      return toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
    }

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static bool TryToGetParam<T>(this IBuilder builder, IModel.Builder.Param toFetch, out T result, T defaultValue = default) {
      if(toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toFetch.Key}, is clamped to the type: {toFetch.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if(builder._tryToGetRawValue(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if(value is T typedValue) {
          result = typedValue;

          return true;
        }

        // if the provided value is null, and this is nullable, return the default value
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if(canBeNull && value == null) {
          result = toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
          return false;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          result = (T)value.CastTo(toFetch.ValueType);
          return true;
        }
        catch(Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. But param has type {value?.GetType().FullName ?? "null"}, and should be type {toFetch.ValueType}.\n{e}");
        }
      }

      result = toFetch.HasDefaultValue ? (T)toFetch.DefaultValue : defaultValue;
      return false;
    }

    /// <summary>
    /// Fetch a param from a collection. The param cannot be left out, and no defaults will be replaced.
    /// </summary>
    public static T GetAndValidateParamAs<T>(this IBuilder builder, IModel.Builder.Param toFetch) {
      if(toFetch.ValueType != null && !toFetch.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toFetch.Key}, is clamped to the type: {toFetch.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if(builder._tryToGetRawValue(toFetch.Key, out object value)) {
        // if the value is of the requested type, return it
        if(value is T typedValue) {
          return typedValue;
        }

        // if the provided value is null, and this is nullable, return the provided null
        bool canBeNull = !toFetch.ValueType.IsValueType || (Nullable.GetUnderlyingType(toFetch.ValueType) != null);
        if(canBeNull && value == null) {
          return default;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          return (T)value.CastTo(toFetch.ValueType);
        }
        catch(Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. But param has type {value?.GetType().FullName ?? "null"}, and should be type {toFetch.ValueType}.\n{e}");
        }
      }

      throw new IModel.Builder.Param.MissingException($"Tried to construct a model without the required param: {toFetch} of type {typeof(T).FullName} being provided. If this is a test, try adding a default value for the empty model for this required param to the Archetype's DefaultEmptyParams field.");
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetParam<T>(this IBuilder builder, IModel.Builder.Param toSet, T value = default) {
      if(builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }
      if(toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }

      builder._add(toSet.Key, value);
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetDefaultParamValue<T>(this IBuilder builder, IModel.Builder.Param toSet, T defaultValue) {
      if(builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }
      if(toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if(!builder._tryToGetRawValue(toSet.Key, out _)) {
        builder._add(toSet.Key, defaultValue);
      }
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetParamToDefault<T>(this IBuilder builder, IModel.Builder.Param toSet) {
      if(builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }
      if(toSet.ValueType != null && !toSet.ValueType.IsAssignableFrom(typeof(T))) {
        throw new IModel.Builder.Param.MissmatchException($"Param {toSet.Key}, is clamped to the type: {toSet.ValueType.FullName}. {typeof(T).FullName} is not a valid type to try to fetch.");
      }
      if(!builder._tryToGetRawValue(toSet.Key, out _)) {
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
      Type valueType = typeof(T);
      if(builder._tryToGetRawValue(paramKey, out object value)) {
        // if the value is of the requested type, return it
        if(value is T typedValue) {
          return typedValue;
        }

        // if the provided value is null, and this is nullable, return the default value
        bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
        if(canBeNull && value == null) {
          return defaultValue;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          return (T)value.CastTo(valueType);
        }
        catch(Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. But param has type {value?.GetType().FullName ?? "null"}, and should be type {valueType}.\n{e}");
        }
      }

      return defaultValue;
    }

    /// <summary>
    /// Fetch a param from a collection, or the default if it's not provided, or the provided is a nullable and null is provided
    /// </summary>
    public static bool TryToGetParam<T>(this IBuilder builder, string paramKey, out T result, T defaultValue = default) {
      Type valueType = typeof(T);
      if(builder._tryToGetRawValue(paramKey, out object value)) {
        // if the value is of the requested type, return it
        if(value is T typedValue) {
          result = typedValue;
          return true;
        }

        // if the provided value is null, and this is nullable, return the default value
        bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
        if(canBeNull && value == null) {
          result = defaultValue;
          return false;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          result = (T)value.CastTo(valueType);
          return true;
        }
        catch(Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. But param has type {value?.GetType().FullName ?? "null"}, and should be type {valueType}.\n{e}");
        }
      }

      result = defaultValue;
      return false;
    }

    /// <summary>
    /// Fetch a param from a collection. The param cannot be left out, and no defaults will be replaced.
    /// </summary>
    public static T GetAndValidateParamAs<T>(this IBuilder builder, string paramKey) {
      Type valueType = typeof(T);
      if(builder._tryToGetRawValue(paramKey, out object value)) {
        // if the value is of the requested type, return it
        if(value is T typedValue) {
          return typedValue;
        }

        // if the provided value is null, and this is nullable, return the provided null
        bool canBeNull = !valueType.IsValueType || (Nullable.GetUnderlyingType(valueType) != null);
        if(canBeNull && value == null) {
          return default;
        }

        // See if this can be cast to the valuetype of the param, and return it if it can.
        try {
          return (T)value.CastTo(valueType);
        }
        catch(Exception e) {
          throw new IModel.Builder.Param.MissmatchException($"Tried to get param as type {typeof(T).FullName}. But param has type {value?.GetType().FullName ?? "null"}, and should be type {valueType}.\n{e}");
        }
      }

      throw new IModel.Builder.Param.MissingException($"Tried to construct a model without the required param: {paramKey} of type {typeof(T).FullName} being provided. If this is a test, try adding a default value for the empty model for this required param to the Archetype's DefaultEmptyParams field.");
    }

    /// <summary>
    /// Add a default value to the param collection if there isn't one set already
    /// </summary>
    public static void SetDefaultParamValue<T>(this IBuilder builder, string parameter, T defaultValue = default) {
      if(builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }
      if(!builder._tryToGetRawValue(parameter, out _)) {
        builder._add(parameter, defaultValue);
      }
    }

    /// <summary>
    /// set a value
    /// </summary>
    public static void SetParam<T>(this IBuilder builder, string parameter, T defaultValue = default) {
      if(builder is IModel.Builder modelBuilder && modelBuilder._isImmutable) {
        throw new AccessViolationException($"Cannot change params on an immutable builder");
      }

      builder._add(parameter, defaultValue);
    }

    #endregion
  }
}