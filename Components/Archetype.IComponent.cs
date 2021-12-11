namespace Meep.Tech.Data {

  public abstract partial class Archetype {

    /// <summary>
    /// A Component for an archetype. Contains data and system logics.
    /// This is the non-generic base class for utility
    /// </summary>
    public interface IComponent
      : Data.IComponent {
    }

    /// <summary>
    /// A Component for an archetype. Contains data and system logics.
    /// </summary>
    public interface IComponent<TComponentBase> 
      : Data.IComponent<TComponentBase>, IComponent
      where TComponentBase: IComponent<TComponentBase>
    {
      
    }
  }
}
