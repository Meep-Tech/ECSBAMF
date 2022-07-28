using System.Collections.Generic;

namespace Meep.Tech.Data {
  /// <summary>
  /// This can produce model builders
  /// </summary>
  public interface IBuilderSource {

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    IBuilder MakeDefaultBuilder();

    /// <summary>
    /// The builder for the base model type of this archetype.
    /// You can override this and add more default props to the return for utility.
    /// </summary>
    IBuilder MakeBuilder(Dictionary<string, object> @params);
  }
}
