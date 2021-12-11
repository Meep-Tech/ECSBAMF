namespace Meep.Tech.Data {

  /// <summary>
  /// Can be used to indicate that this component is restricted to a specific branch of models or archetypes based on the provided base type.
  /// </summary>
  public interface IRestrictedComponent
    : IComponent {

    /// <summary>
    /// The base type this component is restricted to use with.
    /// </summary>
    public virtual System.Type RestrictedTo
      => null;
  }

  /// <summary>
  /// Can be used to indicate that this component is restricted to a specific branch of models or archetypes based on the provided base type.
  /// </summary>
  public interface IRestrictedComponent<TRestrictionBase>
    : IRestrictedComponent {

    /// <summary>
    /// The base type this component is restricted to use with.
    /// </summary>
    System.Type Data.IRestrictedComponent.RestrictedTo
     => typeof(TRestrictionBase);
  }
}
