using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data {
  public partial class Archetype {

    /// <summary>
    /// Can be used to modify existing Archetypes who have components open to external edits 
    /// </summary>
    public abstract class Modifications {

      /// <summary>
      /// This is called after all Archetypes are loaded in their base form from their libaries initially; in mod load order.
      /// These modifications will then run afterwards. also in mod load order.
      /// This is called before finalize is called on all archetypes.
      /// </summary>
      protected internal abstract void Initialize();


      #region Configuration

      /// <summary>
      /// Add the given component to the given archetypes After Archetype Loading and Initialization.
      /// These are added after inital components are added, any components are removed, and before any components are updated
      /// These are called before FinishInitialization on the archetype.
      /// </summary>
      protected static void AddAfterInitialzation(Archetype.IComponent component, params Archetype[] archetypes)
        => AddAfterInitialzation(archetypes, component);

      /// <summary>
      /// Add the given components to the given archetypes After Archetype Loading and Initialization.
      /// These are added after inital components are added, any components are removed, and before any components are updated
      /// These are called before FinishInitialization on the archetype.
      /// </summary>
      protected static void AddAfterInitialzation(IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components) {
        if(Archetype.Loader.IsFinished)
          throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");
        components.ForEach(component
          => archetypes.ForEach(archetype => {
            if(archetype.AllowExternalComponentConfiguration) {
              archetype.AddComponent(component);
            }
          }
        ));
      }

      /// <summary>
      /// Remove the given component from the given archetypes After Archetype Loading and Initialization.
      /// These are removed after inital components are added, and before any extra components added or updated
      /// These are called before FinishInitialization on the archetype.
      /// </summary>
      protected static void RemoveAfterInitialzation<TComponent>(params Archetype[] archetypes)
        where TComponent : Archetype.IComponent<TComponent>
          => RemoveAfterInitialzation(archetypes, Components<TComponent>.Key);

      /// <summary>
      /// Remove the given component from the given archetypes After Archetype Loading and Initialization.
      /// These are removed after inital components are added, and before any extra components added or updated
      /// These are called before FinishInitialization on the archetype.
      /// </summary>
      protected static void RemoveAfterInitialzation(string componentKey, params Archetype[] archetypes)
        => RemoveAfterInitialzation(archetypes, componentKey);

      /// <summary>
      /// Remove the given components from the given archetypes After Archetype Loading and Initialization.
      /// These are removed after inital components are added, and before any extra components added or updated
      /// These are called before FinishInitialization on the archetype.
      /// </summary>
      protected static void RemoveAfterInitialzation(IEnumerable<Archetype> archetypes, params string[] componentKeys) {
        if(Archetype.Loader.IsFinished)
          throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");

        componentKeys.ForEach(componentKey
          => archetypes.ForEach(archetype => {
            if(archetype.AllowExternalComponentConfiguration && archetype.HasComponent(componentKey)) {
              archetype.RemoveComponent(componentKey);
            }
          }
        ));
      }

      /// <summary>
      /// Update the given component in the given archetypes After Archetype Loading and Initialization, if they exist.
      /// These are updated after inital components are added, extra components added, any components are removed, but before UpdateOrAddAfterInitialzation
      /// These are called before FinishInitialization on the archetype.
      /// </summary>
      protected static void UpdateAfterInitialzation<TComponent>(Func<TComponent, TComponent> updateComponent, params Archetype[] archetypes)
        where TComponent : Archetype.IComponent<TComponent> {
        if(Archetype.Loader.IsFinished)
          throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");

        archetypes.ForEach(archetype => {
          if(archetype.AllowExternalComponentConfiguration && archetype.HasComponent<TComponent>()) {
            archetype.UpdateComponent(updateComponent);
          }
        });
      }

      /// <summary>
      /// Remove the given component from the given archetypes After Archetype Loading and Initialization.
      /// This is called after inital components are added, extra components added, any components are removed, and existing components are updated
      /// ... it should be last before FinishInitialization on the archetype.
      /// </summary>
      protected static void AddOrUpdateAfterInitialzation(IEnumerable<Archetype> archetypes, params Archetype.IComponent[] components) {
        if(Archetype.Loader.IsFinished)
          throw new AccessViolationException($"Cannot Modify Archetype Components After Loader is Complete");

        components.ForEach(component
          => archetypes.ForEach(archetype => {
            if(archetype.AllowExternalComponentConfiguration) {
              archetype.AddOrUpdateComponent(component);
            }
          }
        ));
      }

      #endregion
    }
  }
}
