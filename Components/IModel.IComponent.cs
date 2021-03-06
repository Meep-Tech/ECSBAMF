using System;

namespace Meep.Tech.Data {

  public partial interface IModel {

    public partial interface IComponent 
      : Data.IComponent {}

    /// <summary>
    /// A Component for an Model. Contains datas. Logic should usually be kept to Archetypes
    /// </summary>
    public partial interface IComponent<TComponentBase> 
      : IComponent,
        Data.IComponent<TComponentBase>
      where TComponentBase : IComponent<TComponentBase> 
    {

      /// <summary>
      /// Can be used to set the model ctor during initalization.
      /// This should be used in the Static constructor for this type only.
      /// </summary>
      protected static void SetDefaultXBamConstructor(IComponent<TComponentBase>.IFactory factory, Func<IBuilder<TComponentBase>, TComponentBase> constructor) {
        // TODO: I wonder if i can throw an error if this isn't called from the right static ctor
        if ((factory as Data.Archetype).AllowInitializationsAfterLoaderFinalization || !factory.Id.Universe.Loader.IsFinished)
          Components<TComponentBase>.Factory.ModelConstructor = constructor;
        else throw new AccessViolationException($"Cannot modify a sealed component factory: {factory}");
      }
    }
  }
}