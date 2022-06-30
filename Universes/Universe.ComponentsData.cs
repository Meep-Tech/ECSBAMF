using System;
using System.Collections.Generic;

namespace Meep.Tech.Data {

  public partial class Universe {

    /// <summary>
    /// Data for all components in the universe
    /// </summary>
    public class ComponentsData {

      /// <summary>
      /// The number of different loaded model types.
      /// </summary>
      public int Count
        => _baseTypes.Count;

      /// <summary>
      /// Dependencies for different types.
      /// </summary>
      public IReadOnlyDictionary<System.Type, IEnumerable<System.Type>> Dependencies
        => _dependencies; internal Dictionary<System.Type, IEnumerable<System.Type>> _dependencies
          = new();

      /// <summary>
      /// All base types, easily accessable
      /// </summary>
      public IEnumerable<System.Type> All
        => _baseTypes.Values;

      /// <summary>
      /// Cached model base types
      /// </summary>
      internal Dictionary<string, System.Type> _baseTypes
        = new();

      /// <summary>
      /// Cached model base types
      /// </summary>
      internal Dictionary<string, System.Type> _byKey
        = new();

      Universe _universe;

      internal ComponentsData(Universe universe) {
        _universe = universe;
      }

      /// <summary>
      /// Get a component type by it's key
      /// </summary>
      public Type Get(string key)
        => _byKey[key];

      /// <summary>
      /// Get the builder for a given component by type.d
      /// </summary>
      public IComponent.IBuilderFactory GetBuilderFactoryFor(Type type)
        => (IComponent.IBuilderFactory)_universe.Models.GetBuilderFactoryFor(type);

      /// <summary>
      /// Get the builder for a given component by type.d
      /// </summary>
      public IComponent.IBuilderFactory GetBuilderFactoryFor<TComponent>()
        where TComponent : IComponent<TComponent>
          => (IComponent.IBuilderFactory)_universe.Models.GetBuilderFactoryFor<TComponent>();

      /// <summary>
      /// Set the builder factory for a type of component.
      /// TODO: Must be doen during init or static ctor calls
      /// </summary>
      public void SetBuilderFactoryFor<TComponent>(IComponent.IBuilderFactory factory)
        where TComponent : IComponent<TComponent>
          => _universe.Models._factoriesByModelType[typeof(TComponent)] 
            = factory;

      /// <summary>
      /// Get the base model type of this component type.
      /// </summary>
      public System.Type GetComponentBaseType(System.Type type)
        => _baseTypes.TryGetValue(type.FullName, out System.Type foundType)
          ? foundType
          : _baseTypes[type.FullName] = _findComponentBaseType(type);

      /// <summary>
      /// Calculate this model's base model type.
      /// </summary>
      System.Type _findComponentBaseType(System.Type type) {
        var potentialModelBaseType = type;
        // while we have a type to check
        while(potentialModelBaseType != null) {
          // check if this is the final base type, it must be a child of IModel then.
          if(potentialModelBaseType.BaseType == null) {
            if(typeof(IComponent).IsAssignableFrom(potentialModelBaseType)) {
              return potentialModelBaseType;
            }
          }

          potentialModelBaseType = potentialModelBaseType.BaseType;
        }

        throw new NotImplementedException($"System.Type: {type.FullName}, does not have a base Type that inherits from IModel or Model<>.");
      }
    }
  }
}
