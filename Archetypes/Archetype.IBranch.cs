using System;

namespace Meep.Tech.Data {

  public abstract partial class Archetype<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> 
  {
    /// <summary>
    /// 
    /// </summary>
    public interface IBranch {

      Type NewBaseModelType
        => null;
    }

    /// <summary>
    /// Used as shorthand for a type that produces a different model via the Model constructor
    /// This will just set the model constructor of the archetype to the basic activator for the parameterless ctor of TNewBaseModel
    /// </summary>
    public interface IBranch<TNewBaseModel> : IBranch
      where TNewBaseModel : TModelBase
    {

      Type IBranch.NewBaseModelType
        => typeof(TNewBaseModel);
    }
  }
}
