using System;

namespace Meep.Tech.Data {

  public partial interface IComponent<TComponentBase> {

    /// <summary>
    /// If this component has a contract with another componnet.
    /// Contracts are executed after both component types have been added to the same model or archetype.
    /// Use the extension of this under IModel or Archetype instead of extending this base version.
    /// </summary>
    public interface IHasContractWith<TOtherComponent>
      where TOtherComponent : Data.IComponent<TOtherComponent> 
    {

      /// <summary>
      /// Executed when both of these components are added to the same object
      /// </summary>
      public (TComponentBase @this, TOtherComponent other) ExecuteContractWith(TOtherComponent otherComponent);
    }
  }

  public partial interface IComponent {

    /// <summary>
    /// A contract between two components.
    /// This is executed after both component types have been added to the same model or archetype.
    /// </summary>
    public interface IContract<TComponentA, TComponentB>
      where TComponentA : Data.IComponent
      where TComponentB : Data.IComponent {

      /// <summary>
      /// Called to execute the contract.
      /// </summary>
      internal protected (TComponentA a, TComponentB b) _execute(TComponentA a, TComponentB b);
    }
  }

  public partial interface IModel {
    public partial interface IComponent {

      /// <summary>
      /// A contract between two components.
      /// This is executed acter both component types have been added to the same model or archetype.
      /// </summary>
      public struct Contract<TComponentA, TComponentB>
        : Data.IComponent.IContract<TComponentA, TComponentB>
        where TComponentA : IComponent
        where TComponentB : IComponent {
        Func<TComponentA, TComponentB, (TComponentA a, TComponentB b)> _executor;

        (TComponentA a, TComponentB b) IContract<TComponentA, TComponentB>._execute(TComponentA a, TComponentB b) 
          => _executor(a, b);
      }
    }

    public partial interface IComponent<TComponentBase> {

      ///<summary><inheritdoc/></summary>
      public new interface IHasContractWith<TOtherComponent> : Data.IComponent<TComponentBase>.IHasContractWith<TOtherComponent>
        where TOtherComponent : IModel.IComponent<TOtherComponent> {}
    }
  }

  public partial class Archetype {
    public partial interface IComponent {

      /// <summary>
      /// A contract between two components.
      /// This is executed acter both component types have been added to the same model or archetype.
      /// </summary>
      public struct Contract<TComponentA, TComponentB>
        : Data.IComponent.IContract<TComponentA, TComponentB>
        where TComponentA : Archetype.IComponent
        where TComponentB : Archetype.IComponent 
      {
        Func<TComponentA, TComponentB, (TComponentA a, TComponentB b)> _executor;

        (TComponentA a, TComponentB b) IContract<TComponentA, TComponentB>._execute(TComponentA a, TComponentB b)
          => _executor(a, b);
      }
    }

    public partial interface IComponent<TComponentBase> {

      ///<summary><inheritdoc/></summary>
      public new interface IHasContractWith<TOtherComponent> : Data.IComponent<TComponentBase>.IHasContractWith<TOtherComponent>
        where TOtherComponent : Archetype.IComponent<TOtherComponent> { }
    }
  }
}
