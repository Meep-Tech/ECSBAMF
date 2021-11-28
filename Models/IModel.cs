using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data {

  /// <summary>
  /// The base interface for a mutable data model that can be produced by an Archetype.
  /// This is the non generic for Utility only
  /// </summary>
  public interface IModel {

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// </summary>
    public IModel copy() =>
      this.serialize().deserialize();

    /// <summary>
    /// (optional)Finish deserializing the model
    /// </summary>
    internal protected virtual void finishDeserialization() {}
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
    public static Model.SerializedData serialize(this IModel model)
      => throw new System.NotImplementedException();

    /// <summary>
    /// Copy the model by serializing and deserializing it.
    /// Overrideable via IModel.copy()
    /// </summary>
    public static IModel copy(this IModel original)
      => original.copy();
  }
}
