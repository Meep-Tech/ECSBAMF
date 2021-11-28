using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// This is the non-generic for utility reasons.
  /// </summary>
  public partial interface IComponent : IModel {

    /// <summary>
    /// Access to the builder factory for this type of component
    /// </summary>
    public IComponent.IBuilderFactory Factory
      => Components.GetBuilderFactoryFor(GetType());

    /// <summary>
    /// Access to the builder factory for this type of component
    /// </summary>
    string Key
      => Factory.Key;
  }

  /// <summary>
  /// The base class for modular data holders for models and archetypes
  /// </summary>
  public partial interface IComponent<TComponentBase> : IModel<TComponentBase>, IComponent 
    where TComponentBase : IComponent<TComponentBase>  
  {
  }
}
