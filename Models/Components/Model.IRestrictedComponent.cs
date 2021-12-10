namespace Meep.Tech.Data {

  public partial class Model {
    /// <summary>
    /// Can be used to indicate that this component is restricted to specific types of models based on the provided base model type.
    /// </summary>
    public interface IRestrictedComponent<TModelBase>
      : Data.IRestrictedComponent<TModelBase>,
        IRestrictedComponent,
        IComponent
      where TModelBase : IModel 
    {

      /// <summary>
      /// Check if this is compatable with a model
      /// </summary>
      bool IRestrictedComponent.IsCompatableWith(IModel model)
        => model is TModelBase;
    }

    /// <summary>
    /// Can be used to indicate that this component is restricted to specific types of models based on the provided base model type.
    /// Base generic functionality
    /// </summary>
    public interface IRestrictedComponent
      : Data.IRestrictedComponent, 
        IComponent
    {

      /// <summary>
      /// Check if this is compatable with a model
      /// </summary>
      public virtual bool IsCompatableWith(IModel model)
        => false;
    }
  }
}