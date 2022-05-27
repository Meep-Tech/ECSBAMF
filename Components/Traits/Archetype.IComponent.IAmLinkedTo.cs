namespace Meep.Tech.Data {

  public abstract partial class Archetype {

    public partial interface IComponent {

      /// <summary>
      /// Can be used to link an archetype component to a model component
      /// </summary>
      public interface IAmLinkedTo<TLinkedModelComponent>
        : ILinkedComponent,
          Archetype.IComponent
        where TLinkedModelComponent : IModel.IComponent<TLinkedModelComponent> {

        /// <summary>
        /// Build and get a default model component that is linked to this archetype component.
        /// This behavior can be overriden by default if you choose. It could even just be a ctor call.
        /// </summary>
        public new TLinkedModelComponent BuildDefaultModelComponent(IModel.Builder parentModelBuilder, Universe universe = null)
          => ((universe.Components.GetBuilderFactoryFor<TLinkedModelComponent>() ?? Components<TLinkedModelComponent>.BuilderFactory)
            as Data.IComponent<TLinkedModelComponent>.BuilderFactory)
             .Make((IBuilder<TLinkedModelComponent>)parentModelBuilder);

        /// <summary>
        /// Build and get a default model component that is linked to this archetype component.
        /// </summary>
        IModel.IComponent ILinkedComponent.BuildDefaultModelComponent(IModel.Builder parentModelBuilder, Universe universe)
          => BuildDefaultModelComponent(parentModelBuilder, universe);
      }

      /// <summary>
      /// Can be used to link an archetype component to a model component
      /// </summary>
      public interface ILinkedComponent
          : Archetype.IComponent {

        /// <summary>
        /// Build and get a default model component that is linked to this archetype component.
        /// </summary>
        public IModel.IComponent BuildDefaultModelComponent(IModel.Builder parentModelBuilder, Universe universe = null)
          => null;
      }
    }
  }
}
