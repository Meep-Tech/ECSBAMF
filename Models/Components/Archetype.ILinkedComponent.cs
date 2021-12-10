using System.Linq;

namespace Meep.Tech.Data {

  public abstract partial class Archetype {

    /// <summary>
    /// Can be used to link an archetype component to a model component
    /// </summary>
    public interface ILinkedComponent<TLinkedModelComponent>
      : ILinkedComponent,
        IComponent
      where TLinkedModelComponent : Model.IComponent<TLinkedModelComponent>
    {

      /// <summary>
      /// Build and get a default model component that is linked to this archetype component.
      /// This behavior can be overriden by default if you choose. It could even just be a ctor call.
      /// </summary>
      public new TLinkedModelComponent BuildDefaultModelComponent(Model.Builder parentModelBuilder)
        => Components<TLinkedModelComponent>.BuilderFactory.Make(parentModelBuilder.AsEnumerable());

      /// <summary>
      /// Build and get a default model component that is linked to this archetype component.
      /// </summary>
      Model.IComponent ILinkedComponent.BuildDefaultModelComponent(Model.Builder parentModelBuilder)
        => BuildDefaultModelComponent(parentModelBuilder);
    }

    /// <summary>
    /// Can be used to link an archetype component to a model component
    /// </summary>
    public interface ILinkedComponent
        : IComponent
    {

      /// <summary>
      /// Build and get a default model component that is linked to this archetype component.
      /// </summary>
      public Model.IComponent BuildDefaultModelComponent(Model.Builder parentModelBuilder)
        => null;
    }
  }
}
