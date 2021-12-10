namespace Meep.Tech.Data {

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// This is the non generic for Utility only
  /// </summary>
  public interface IModel {

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// </summary>
    public IModel Copy() =>
      this.Serialize().Deserialize();

    /// <summary>
    /// (optional)Finish deserializing the model
    /// </summary>
    internal protected virtual void FinishDeserialization() {}
  }

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// </summary>
  public interface IModel<TModelBase>
    : IModel
    where TModelBase : IModel<TModelBase> {}

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// </summary>
  public interface IModel<TModelBase, TArchetypeBase>
    : IModel<TModelBase>
    where TModelBase : IModel<TModelBase, TArchetypeBase>
    where TArchetypeBase : Archetype<TModelBase, TArchetypeBase>
  {}

  public static class IModelExtensions {

    /// <summary>
    /// Turn the model into a serialized data object.
    /// </summary>
    public static Model.SerializedData Serialize(this IModel model)
      => throw new System.NotImplementedException();

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// Overrideable via IModel.copy()
    /// </summary>
    public static IModel Copy(this IModel original)
      => original.Copy();
  }
}
