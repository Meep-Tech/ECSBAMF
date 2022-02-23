namespace Meep.Tech.Data {

  public partial class Universe {

    /// <summary>
    /// A Type that can be added as context to a universe.
    /// </summary>
    public class ExtraContext {

      /// <summary>
      /// Code that's executed on initialization of the loader
      /// </summary>
      internal protected virtual void OnLoaderInitialize() { }

      /// <summary>
      /// Code that's executed on finalization of the loader.
      /// </summary>
      internal protected virtual void OnLoaderFinalize() { }

      /// <summary>
      /// Code that's executed on finalization of a new type of model.
      /// </summary>
      internal protected virtual void OnModelTypeRegistered(System.Type modelType, IModel defaultModel) { }
    }
  }
}
