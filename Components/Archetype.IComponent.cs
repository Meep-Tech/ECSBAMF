using System;

namespace Meep.Tech.Data {

  public abstract partial class Archetype {

    /// <summary>
    /// A Component for an archetype. Contains data and system logics.
    /// This is the non-generic base class for utility
    /// </summary>
    public interface IComponent
      : Data.IComponent {

      /// <summary>
      /// Archetype component's shouldn't need to know about the universe they're in.
      /// </summary>
      Universe IModel.Universe
        => throw new NotImplementedException("Archetype component's shouldn't need to know about the universe they're in by default. This can be overriden via IModel.Universe.get");
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
