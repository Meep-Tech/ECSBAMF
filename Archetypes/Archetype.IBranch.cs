/*using System;

namespace Meep.Tech.Data {

  public abstract partial class Archetype<TModelBase, TArchetypeBase> where TModelBase : IModel<TModelBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase> 
  {
    /// <summary>
    /// Not Implimented
    /// </summary>
    public interface IBranch {

      /// <summary>
      /// Not Implimented
      /// </summary>
      Type NewBaseModelType
        => null;
    }

    /// <summary>
    /// Not Implimented
    /// Use The BranchAttribute instead.
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
}*/
