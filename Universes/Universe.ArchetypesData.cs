using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Data {

  public partial class Universe {

    /// <summary>
    /// Used to hold the data for all archetypes
    /// </summary>
    public class ArchetypesData {

      /// <summary>
      /// link to the parent universe
      /// </summary>
      Universe _universe;

      /// <summary>
      /// All archetypes:
      /// </summary>
      public Archetype.Collection All {
        get;
      }

      /// <summary>
      /// All registered Archetype Identities
      /// </summary>
      public IEnumerable<Archetype.Identity> Ids
        => _ids.Values;
      Dictionary<string, Archetype.Identity> _ids
        = new();

      /// <summary>
      /// Ids, indexed by external id value
      /// </summary>
      public IReadOnlyDictionary<string, Archetype.Identity> Id
        => _ids;

      /// <summary>
      /// Dependencies for different types.
      /// </summary>
      public IReadOnlyDictionary<System.Type, IEnumerable<System.Type>> Dependencies
        => _dependencies; internal Dictionary<System.Type, IEnumerable<System.Type>> _dependencies
          = new();

      /// <summary>
      /// Root types for archetypes based on a model type fullname.
      /// </summary>
      internal Dictionary<string, System.Type> _rootArchetypeTypesByBaseModelType
        = new();

      /// <summary>
      /// All Root Archetype Collections.
      /// Doesn't include Branch collections.
      /// </summary>
      public IEnumerable<Archetype.Collection> RootCollections {
        get => _collectionsByRootArchetype.Values;
      } internal readonly Dictionary<string, Archetype.Collection> _collectionsByRootArchetype
        = new();

      /// <summary>
      /// All Archetype Collections:
      /// </summary>
      public IEnumerable<Archetype.Collection> Collections {
        get => RootCollections.Concat(_branchedCollectionsByBranchArchetype.Values);
      } readonly Dictionary<string, Archetype.Collection> _branchedCollectionsByBranchArchetype
        = new();

      internal ArchetypesData(Universe universe) {
        _universe = universe;
        All = new Archetype.Collection(universe);
        _collectionsByRootArchetype.Add(typeof(Archetype).FullName, All);
      }

      /// <summary>
      /// Get a collection registered to an archetype root:
      /// </summary>
      public Archetype.Collection GetCollectionFor(Archetype root)
        => _collectionsByRootArchetype.TryGetValue(root.Id.Key, out Archetype.Collection collection)
          ? collection
          // recurse until it's found. This should throw a null exception eventually if one isn't found.
          : GetCollectionFor(root.Type.BaseType);

      /// <summary>
      /// Get a collection registered to an archetype root:
      /// </summary>
      public bool _tryToGetCollectionFor(System.Type root, out Archetype.Collection found) {
        if(_collectionsByRootArchetype.TryGetValue(root?.FullName, out Archetype.Collection collection)) {
          found = collection;
          return true;
        } // stop if we reached the base
        else if(root.Equals(typeof(object)) || root?.BaseType is null) {
          found = null;
          return false;
        }
        // recurse until it's found.
        else {
          return _tryToGetCollectionFor(root.BaseType, out found);
        }
      }

      /// <summary>
      /// Get a collection registered to an archetype type:
      /// </summary>
      public Archetype.Collection GetCollectionFor(System.Type root)
        => _collectionsByRootArchetype.TryGetValue(root?.FullName ?? "", out Archetype.Collection collection)
          ? collection
          // recurse until it's found. This should throw a null exception eventually if one isn't found.
          : GetCollectionFor(root.BaseType);

      /// <summary>
      /// Get the "default" archetype or factory for a given model type.
      /// </summary>
      public Archetype GetDefaultForModelOfType<TModelBase>()
        where TModelBase : IModel
          => GetDefaultForModelOfType(typeof(TModelBase));

      /// <summary>
      /// Get the "default" archetype or factory for a given model type.
      /// </summary>
      public Archetype GetDefaultForModelOfType(System.Type modelBaseType) {
        Archetype archetype;
        if (modelBaseType.IsAssignableToGeneric(typeof(IModel<,>))) {
          if (_rootArchetypeTypesByBaseModelType.TryGetValue(modelBaseType.FullName, out var rootArchetypeType)) {
            if ((archetype = rootArchetypeType.TryToGetAsArchetype()) != null) {
              return archetype;
            }

            // if we couldn;t make the type into an archetype (it may be a base type), we need to get any default archetype from it's collection:
            Archetype.Collection collection = GetCollectionFor(rootArchetypeType);
            if (collection is null) {
              throw new KeyNotFoundException($"Could not find an archetype collection the model Type: {modelBaseType}");
            }
            if ((archetype = collection.FirstOrDefault()) != null) {
              return archetype;
            } else {
              throw new KeyNotFoundException($"Could not find a default archetype for the model Type: {modelBaseType}, in collection: {GetCollectionFor(_rootArchetypeTypesByBaseModelType[modelBaseType.FullName]).GetType().ToFullHumanReadableNameString()}");
            }
          } else throw new KeyNotFoundException($"Could not find a Root Archetype for the Base Model Type: {modelBaseType}");
        } else {
          return _universe.Models.GetBuilderFactoryFor(modelBaseType) as Archetype;
        }
      }

      /// <summary>
      /// Get the "default" archetype or factory for a given model type.
      /// </summary>
      /*public Archetype GetDefaultForModelOfType(System.Type modelBaseType)
        => modelBaseType.IsAssignableToGeneric(typeof(IModel<,>))
          ? _rootArchetypeTypesByBaseModelType[modelBaseType.FullName].TryToGetAsArchetype()
            ?? GetCollectionFor(_rootArchetypeTypesByBaseModelType[modelBaseType.FullName]).FirstOrDefault()
              ?? throw new KeyNotFoundException($"Could not find a default archetype for the model Type: {modelBaseType}, in collection:{GetCollectionFor(_rootArchetypeTypesByBaseModelType[modelBaseType.FullName])}")
          : _universe.Models.GetBuilderFactoryFor(modelBaseType) as Archetype;*/

      internal void _registerArchetype(Archetype archetype, Archetype.Collection collection) {
        // Register to it's id
        archetype.Id.Archetype = archetype;

        // Register to collection:
        collection.Add(archetype);
        archetype.TypeCollection = collection;

        // Add to All:
        All.Add(archetype);
        _ids.Add(archetype.Id.Key, archetype.Id);
      }

      internal void _unRegisterArchetype(Archetype archetype) {
        // remove from all:
        All._remove(archetype);
        _ids.Remove(archetype.Id.Key);

        // remove metadata:
        foreach((string modelType, Type archetypeType) in _rootArchetypeTypesByBaseModelType.ToArray()) {
          if (archetypeType == archetype.Type) {
            _rootArchetypeTypesByBaseModelType.Remove(modelType);
          }
        }

        // Register to collection:
        archetype.TypeCollection._remove(archetype);
        archetype.TypeCollection = null;

        // de-register it from it's id
        archetype.Id.Archetype = null;
      }

      internal void _registerCollection(Archetype.Collection collection, Type rootArchetypeType = null) {
        if(!(_universe?.Archetypes is null || _universe?.Models is null || _universe?.Components is null)) {
          if(collection is Archetype.Collection.IBranch) {
            _branchedCollectionsByBranchArchetype.Add(rootArchetypeType.FullName, collection);
          } else
           _collectionsByRootArchetype.Add(rootArchetypeType?.FullName ?? "_all", collection);
        }
      }

      /// <summary>
      /// TODO: impliment?
      /// </summary>
      internal void _unRegisterCollection(Archetype.Collection collection, Type rootArchetypeType = null) {
        if (collection is Archetype.Collection.IBranch) {
          _branchedCollectionsByBranchArchetype.Remove(rootArchetypeType.FullName);
        }
        else
          _collectionsByRootArchetype.Remove(rootArchetypeType?.FullName ?? "_all");
      }
    }
  }
}
