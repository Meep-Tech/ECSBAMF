using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  public partial interface IComponent<TComponentBase> {

    /// <summary>
    /// If this component has a contract with another componnet.
    /// Contracts are executed after both component types have been added to the same model or archetype.
    /// Use the extension of this under IModel or Archetype instead of extending this base version.
    /// </summary>
    public interface IHaveContractWith<TOtherComponent>
      : IHaveContract
      where TOtherComponent : Data.IComponent<TOtherComponent> 
    {

      /// <summary>
      /// Executed when both of these components are added to the same object
      /// </summary>
      internal protected (TComponentBase @this, TOtherComponent other) ExecuteContractWith(TOtherComponent otherComponent);
    }
  }

  public partial interface IComponent {

    /// <summary>
    /// Base interface for having a contract.
    /// Use the extension of this under IModel or Archetype instead of extending this base version.
    /// </summary>
    public interface IHaveContract : Data.IComponent {

      internal static Dictionary<System.Type, Dictionary<System.Type, Func<Data.IComponent, Data.IComponent, (Data.IComponent a, Data.IComponent b)>>> _contracts
        = new();
    }
  }

  public partial interface IModel {

    public partial interface IComponent<TComponentBase> {

      ///<summary><inheritdoc/></summary>
      public new interface IHaveContractWith<TOtherComponent> : Data.IComponent<TComponentBase>.IHaveContractWith<TOtherComponent>
        where TOtherComponent : IModel.IComponent<TOtherComponent> {}
    }
  }

  public partial class Archetype {

    public partial interface IComponent<TComponentBase> {

      ///<summary><inheritdoc/></summary>
      public new interface IHaveContractWith<TOtherComponent> : Data.IComponent<TComponentBase>.IHaveContractWith<TOtherComponent>
        where TOtherComponent : Data.Archetype.IComponent<TOtherComponent> { }
    }
  }
}
