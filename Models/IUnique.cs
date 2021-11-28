namespace Meep.Tech.Data {

  /// <summary>
  /// A Model with a unique id
  /// </summary>
  public interface IUnique : IModel {

    /// <summary>
    /// Params for Unique models
    /// </summary>
    public static class Params {

      /// <summary>
      /// Unique id for any type of model
      /// </summary>
      public static Model.Builder.Param UniqueId {
        get;
      } = new Model.Builder.Param("Unique Id");
    }

    /// <summary>
    /// The Unique Id of this Item
    /// </summary>
    public string id {
      get;
      internal set;
    }

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// </summary>
    public IUnique copy(bool newUniqueId = true) {
      IUnique copy = (IUnique)(this as IModel).copy();
      if(newUniqueId) {
        copy.resetUniqueId();
      }

      return copy;
    }
  }

  public static class IUniqueExtensions {

    /// <summary>
    /// Changes the unique id of this model.
    /// This can break saving/linking!
    /// </summary>
    internal static void resetUniqueId(this IUnique original) {
      original.id = RNG.GetNextUniqueId();
    }

    /// <summary>
    /// Copy a unique model, with a new unique id
    /// Override via IUnique.copy(bool)
    /// </summary>
    public static IUnique copy(this IUnique original, bool newUniqueId = true) 
      => original.copy(newUniqueId);
  }
}