namespace Meep.Tech.Data {

  public partial class Universe {

    /// <summary>
    /// A Type that can be added as context to a universe.
    /// </summary>
    public class ExtraContext {

      /// <summary>
      /// The universe this context is for.
      /// </summary>
      public Universe Universe {
        get;
        internal set;
      }

      /// <summary>
      /// Code that's executed on initialization of the loader, before any types are loaded
      /// </summary>
      internal protected virtual void OnLoaderInitialize() { }

      /// <summary>
      /// Code that's executed before final initialization but after registration of a new type of model.
      /// </summary>
      internal protected virtual void OnModelTypeWasRegistered(System.Type modelType, IModel defaultModel) { }

      /// <summary>
      /// Code that's executed after a new type of model is initialized.
      /// </summary>
      internal protected virtual void OnModelTypeWasInitialized(System.Type modelType) { }

      /// <summary>
      /// Code that's executed on finalization of a new type of model.
      /// </summary>
      internal protected virtual void OnArchetypeWasInitialized(System.Type archetypeType, Archetype archetype) { }

      /// <summary>
      /// Code that's executed when the initialization of all models, components, enums, and archetypes is complete.
      /// This happens before modifications.
      /// </summary>
      internal protected virtual void OnAllTypesInitializationComplete() { }

      /// <summary>
      /// Code that's executed when modifications are done loading, before all types are finalized.
      /// </summary>
      internal protected virtual void OnModificationsComplete() { }

      /// <summary>
      /// Code that's executed on finalization of the loader, after all types are already finalized and before the loader is sealed.
      /// </summary>
      internal protected virtual void OnLoaderFinalize() { }

      /// <summary>
      /// Occurs when an archetype is un-loaded.
      /// </summary>
      internal protected virtual void OnUnload(Archetype archetype) { }
    }
  }
}
