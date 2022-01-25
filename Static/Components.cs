using System;

namespace Meep.Tech.Data {

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Components {

    /// <summary>
    /// The default universe to use for models
    /// </summary>
    public static Universe DefaultUniverse {
      get => _defaultUniverseOverride ??= Archetypes.DefaultUniverse;
      set => _defaultUniverseOverride = value;
    } private static Universe _defaultUniverseOverride;

    /// <summary>
    /// Get the builder for a given component by type.d
    /// </summary>
    public static IComponent.IBuilderFactory GetBuilderFactoryFor(Type type)
      => DefaultUniverse.Components.GetBuilderFactoryFor(type);

    /// <summary>
    /// Get the base model type of this model type.
    /// </summary>
    public static System.Type GetComponentBaseType(this System.Type type)
      => DefaultUniverse.Components.GetComponentBaseType(type);
  }

  /// <summary>
  /// Static data values for components
  /// </summary>
  public static class Components<TComponent> 
    where TComponent : Data.IComponent<TComponent> 
  {

    /// <summary>
    /// The key for this type of component.
    /// This is based on the base model type's name.
    /// There should only be one component per key on a model.
    /// </summary>
    public static string Key
      => BuilderFactory.Key;

    /// <summary>
    /// Builder instance for this type of component.
    /// You can use this to set a custom builder for this type of component and it's children.
    /// </summary>
    public static IComponent<TComponent>.BuilderFactory BuilderFactory {
      get => (IComponent<TComponent>.BuilderFactory)
        Components.DefaultUniverse.Components
          .GetBuilderFactoryFor<TComponent>();
      set {
        Components.DefaultUniverse.Components
          .SetBuilderFactoryFor<TComponent>(value);
      }
    }
  }
}