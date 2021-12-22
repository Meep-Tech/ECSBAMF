using System;

namespace Meep.Tech.Data {

  /// <summary>
  /// These make models
  /// </summary>
  public partial interface IFactory {

    /// <summary>
    /// The Id of this Archetype.
    /// </summary>
    public Archetype.Identity Id {
      get;
    }

    /// <summary>
    /// Overrideable Model Constructor
    /// </summary>
    Func<IBuilder, IModel> ModelConstructor {
      get;
      internal set;
    }
  }
}
