namespace Meep.Tech.Data {

  public partial class Model {

    /// <summary>
    /// A Component for an Model. Contains datas. Logic should usually be kept to Archetypes
    /// </summary>
    public interface IComponent<TComponentBase> : Data.IComponent<TComponentBase>
      where TComponentBase : IComponent<TComponentBase> 
    {

    }
  
  }
}