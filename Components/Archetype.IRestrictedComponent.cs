namespace Meep.Tech.Data {

  public abstract partial class Archetype {

    /// <summary>
    /// Can be used to indicate that this component is restricted to a specific branch of archetypes based on the provided base archetype.
    /// </summary>
    public interface IRestrictedComponent<TArchetypeBase> 
      : Data.IRestrictedComponent<TArchetypeBase>,
        IRestrictedComponent,
        IComponent
      where TArchetypeBase : Archetype {

      /// <summary>
      /// Check if this is compatable with an archetype
      /// </summary>
      bool Archetype.IRestrictedComponent.IsCompatableWith(Archetype archetype)
        => archetype is TArchetypeBase;
    }

    /// <summary>
    /// Can be used to indicate that this component is restricted to a specific branch of archetypes based on the provided base archetype.
    /// Base Generic Type.
    /// </summary>
    public interface IRestrictedComponent 
      : Data.IRestrictedComponent,
        IComponent
    {

      /// <summary>
      /// Check if this is compatable with an archetype
      /// </summary>
      public virtual bool IsCompatableWith(Archetype archetype)
        => false;
    }
  }
}
