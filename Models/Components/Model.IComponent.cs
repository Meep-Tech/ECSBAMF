namespace Meep.Tech.Data {

  public partial class Model {

    public partial interface IComponent : Data.IComponent {}

    /// <summary>
    /// A Component for an Model. Contains datas. Logic should usually be kept to Archetypes
    /// </summary>
    public interface IComponent<TComponentBase> 
      : IComponent,
        Data.IComponent<TComponentBase>
      where TComponentBase : IComponent<TComponentBase> 
    {

    }
  }
}