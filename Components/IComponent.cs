namespace Meep.Tech.Data {

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// This is the non-generic for utility reasons.
  /// </summary>
  public partial interface IComponent : IModel {

    /// <summary>
    /// Access to the builder factory for this type of component
    /// </summary>
    public Data.IComponent.IBuilderFactory Factory
      => Components.GetBuilderFactoryFor(GetType());

    /// <summary>
    /// A key used to index this type of component. There can only be one componet per key on a model or archetype
    /// This is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
    /// </summary>
    string Key
      => Factory.Key;

    /// <summary>
    /// Default configuration
    /// </summary>
    IModel IModel.Configure(IBuilder builder)
      => this;

    /// <summary>
    /// optional finalization logic for components after the model has been finalized
    /// </summary>
    public Data.IComponent FinalizeComponentAfterParent(IModel parent)
      => this;
  }

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent 
    where TComponentBase : IComponent<TComponentBase> 
  {

    /// <summary>
    /// This can be overriden if you want, but by default, struct based components don't have universe info at hand
    /// </summary>
    Universe IModel<TComponentBase>.Universe
      => null;
  }

  public static class ComponentExtensions {

    /// <summary>
    /// Helper function to fetch the key for this component type
    /// A key used to index this type of component. There can only be one componet per key on a model or archetype
    /// The Key is the Component Base Type (the type that inherits initially from one of the IComponent interfaces)
    /// </summary>
    public static string GetKey(this IComponent component)
    => component.Key;
  }
}
