using Meep.Tech.Noise;

namespace Meep.Tech.Data {

  /// <summary>
  /// An unique object with a unique id.
  /// </summary>
  public interface IUnique : IModel {

    /// <summary>
    /// The Unique Id of this Item
    /// </summary>
    public string Id {
      get;
      internal protected set;
    }

    /// <summary>
    /// If the id should be automatically set by the IModel.Builder
    /// </summary>
    bool AutoSetIdOnBuild
      => true;

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// </summary>
    public IUnique Copy(bool newUniqueId = true) {
      IUnique copy = (IUnique)(this as IModel).Copy();
      if(newUniqueId) {
        copy._resetUniqueId();
      }

      return copy;
    }
  }

  /// <summary>
  /// Extensions and helpers for IUnique
  /// </summary>
  public static class IUniqueExtensions {

    /// <summary>
    /// Get the unique id
    /// </summary>
    public static string GetUniqueId(this IUnique unique)
      => unique.Id;

    /// <summary>
    /// Changes the unique id of this model.
    /// This can break saving/linking!
    /// </summary>
    internal static void _resetUniqueId(this IUnique original) {
      original.Id = RNG.GenerateNextGuid();
    }

    /// <summary>
    /// Copy a unique model, with a new unique id
    /// Override via IUnique.copy(bool)
    /// </summary>
    public static IUnique Copy(this IUnique original, bool newUniqueId = true) 
      => original.Copy(newUniqueId);
  }
}