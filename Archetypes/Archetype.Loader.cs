using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Meep.Tech.Data {

  public abstract partial class Archetype {

    /// <summary>
    /// Loads archetypes.
    /// </summary>
    public static partial class Loader {

      /// <summary>
      /// Settings for the loader
      /// </summary>
      public static class Settings {

        /// <summary>
        /// Assemblies that should be included in the loading that are built in.
        /// This helps prevent assemblies from not being loaded yet on initial searches
        /// </summary>
        public static List<Assembly> PreLoadAssemblies
          = new List<Assembly>();

        /// <summary>
        /// If a single archetype not being initialized should throw a fatal.
        /// </summary>
        public static bool FatalOnCannotInitializeArchetype
          = false;

        /// <summary>
        /// The prefix to limit assemblies to for loading archetypes
        /// </summary>
        public static string ArchetypeAssembliesPrefix
          = "";

        /// <summary>
        /// How many times to re-run initialization to account for types that require others
        /// </summary>
        public static short InitializationAttempts
          = 10;

        /// <summary>
        /// How many times to attempt to run finalization on remaining initializing types
        /// </summary>
        public static short FinalizationAttempts
          = 1;

        /// <summary>
        /// How many times to loop though components to initialize them accounting for dependency misses
        /// </summary>
        public static int ComponentInitializationAttempts
          = 4;

        /// <summary>
        /// Overrideable bool to allow runtime registrations of types that set AllowSubtypeRuntimeRegistrations to true.
        /// </summary>
        public static bool AllowRuntimeTypeRegistrations
          = false;

        /// <summary>
        /// The location of archetype librararies and mod extensions
        /// </summary>
        public static string ModsRootFolderLocation
          = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mods");

        /// <summary>
        /// A pre-settable setting for specifying how to order certain mods for loading.
        /// This will throw if there's a conflict with order.json
        /// </summary>
        public static Dictionary<int, string> PreOrderedModAssemblyFiles
          = new Dictionary<int, string>();

        /// <summary>
        /// Assembled mod load order.
        /// </summary>
        internal static IReadOnlyList<string> ModLoadOrder {
          get;
        } internal static List<string> _modLoadOrder {
          get;
        } = new List<string>();

        /// <summary>
        /// Prevents a type that inherits from Archetype<,> or IModel<> from being built as an archetype during initial loading. this is NOT inherited.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
        public class DoNotBuildInInitialLoadAttribute
          : Attribute { }

        /// <summary>
        /// Prevents a type that inherits from Archetype<,> or IModel<> and it's inherited types from being built into an archetype during initial loading.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
        public class DoNotBuildThisOrChildrenInInitialLoadAttribute
          : DoNotBuildInInitialLoadAttribute { }
      }

      /// <summary>
      /// If all archetypes have been initialized and the loader is finished.
      /// Once this is true, you cannot modify archetypes or their collections anymore.
      /// </summary>
      public static bool IsFinished {
        get;
        private set;
      } = false;

      /// <summary>
      /// The types we need to construct and map data to
      /// </summary>
      static List<System.Type> _uninitializedArchetypes
        = new List<System.Type>();

      /// <summary>
      /// The types we need to construct and map data to
      /// </summary>
      static List<System.Type> _uninitializedModels
        = new List<System.Type>();

      /// <summary>
      /// The types we need to construct and map data to
      /// </summary>
      static List<System.Type> _uninitializedComponents
        = new List<System.Type>();

      /// <summary>
      /// The types we need to construct and map data to
      /// </summary>
      static List<Archetype> _initializedArchetypes
        = new List<Archetype>();

      /// <summary>
      /// How many initalization attempts are remaining
      /// </summary>
      static int _remainingInitializationAttempts
        = Settings.InitializationAttempts;

      /// <summary>
      /// How many finalization attempts are remaining
      /// </summary>
      static int _remainingFinalizationAttempts
        = Settings.FinalizationAttempts;

      /// <summary>
      /// The types we need to construct and map data to
      /// </summary>
      static HashSet<string> _initializedDbContexts
        = new HashSet<string>();

      /// <summary>
      /// Try to load all archetypes, using the Settings
      /// </summary>
      public static void Initialize() {
        _initalize();

        while(_remainingInitializationAttempts-- > 0 && _uninitializedArchetypes.Count > 0) {
          _tryToCompleteInitialization();
        }

        _applyModifications();

        while(_remainingFinalizationAttempts-- > 0 && _initializedArchetypes.Count > 0) {
          _tryToFinishAllInitalizedTypes();
        }

        foreach(Type systemType in _uninitializedModels) {
          _registerModelType(systemType);
        }

        foreach(Type systemType in _uninitializedComponents) {
          _registerComponentType(systemType);
        }

        _finalize();
      }

      /// <summary>
      /// Set up initial settings.
      /// </summary>
      static void _initalize() {
        // Setup newtonsoft json.net
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings() {
          ContractResolver = new DefaultContractResolver() {
            IgnoreSerializableAttribute = false
          }
        };

        Model.Serializer.Settings.ComponentJsonSerializerSettings = new JsonSerializerSettings() {
          ContractResolver = new Model.IComponent.ShouldSerializeTypeInComponentsContractResolver() {
            IgnoreSerializableAttribute = false
          }
        };

        Model.Serializer.Settings.DbContext 
          ??= new Model.Serializer.DbContext(
            new DbContextOptions<Model.Serializer.DbContext>()
          );

        // pre-load
        Settings.PreLoadAssemblies.Count();

        // order assemblies according to the config.json.

        // get all types
        (_uninitializedArchetypes,
           _uninitializedModels,
          _uninitializedComponents
        ) = _getAllBuildableTypes();
      }

      /// <summary>
      /// Load all the mods from the mod folder
      /// </summary>
      static void _loadMods() {
        string loadOrderFile = Path.Combine(Settings.ModsRootFolderLocation, "order.json");
        if(File.Exists(loadOrderFile)) {
          JsonConvert.DeserializeObject<List<LoadOrderItem>>(File.ReadAllText(loadOrderFile));
        }
      }

      struct LoadOrderItem {
        public int priority {
          get;
        }

        public string assemblyFileName {
          get;
        }
      }

      /// <summary>
      /// Register a new type of model.
      /// </summary>
      static void _registerModelType(Type systemType) {
        _testBuildDefaultModel(systemType);
        // TODO: this may actually work fine?
        // TODO: this shouldn't attach this type, it should find the 
        // first base model type with it's own db settings, or just use the base model type.
        Model.Serializer.Settings.DbContext.Attach(systemType);

        _initializedDbContexts.Add(systemType.Name);
      }

      /// <summary>
      /// Register types of components
      /// </summary>
      /// <param name="systemType"></param>
      static void _registerComponentType(Type systemType) {
        _testBuildDefaultComponent(systemType);
      }

      /// <summary>
      /// Test build a model of the given type using it's default archetype or builder.
      /// </summary>
      static void _testBuildDefaultModel(Type systemType) {
        Archetype defaultFactory;
        if(systemType.IsAssignableToGeneric(typeof(Model<>))) {
          defaultFactory = Models.GetBuilderFactoryFor(systemType) as Archetype;
        }
        else {
          defaultFactory = Archetypes.All._byModelBaseType[systemType.FullName];
        }

        if(defaultFactory == null) {
          throw new Exception($"Could not make a default model for model of type: {systemType.FullName}. Could not fine a default BuilderFactory or Archetype to build it with.");
        }

        try {
          defaultFactory.MakeDefault();
        }
        catch {
          throw new Exception($"Could not make a default model for model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}.");
        }
      }

      /// <summary>
      /// Test build a model of the given type using it's default archetype or builder.
      /// </summary>
      static void _testBuildDefaultComponent(Type systemType) {
        if(!(Data.Components.GetBuilderFactoryFor(systemType) is Archetype defaultFactory)) {
          throw new Exception($"Could not make a default component model of type: {systemType.FullName}. Could not fine a default BuilderFactory to build it with.");
        }

        try {
          defaultFactory.MakeDefault();
        }
        catch {
          throw new Exception($"Could not make a default component model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}.");
        }
      }

      /// <summary>
      /// Get all types that this loader knows how to build from the loaded assemblies.
      /// </summary>
      static (List<Type> archetypes, List<Type> models, List<Type> components) _getAllBuildableTypes() {
        (List<Type> archetypes, List<Type> models, List<Type> components) @return = (
          new List<Type>(),
          new List<Type>(),
          new List<Type>()
        );

        // TODO: allow the assemblies to somehow apply a load order.
        // Maybe provide their own weight, or an ini/json file with weights/settings
        // after that we should also get the Archetype.Modifier classes from each assembly if they exist.

        // For each loaded assembly
        foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()
          // ... that is not dynamic, and that matches any naming requirements
          .Where(assembly => !assembly.IsDynamic
            && assembly.GetName().FullName.StartsWith(Settings.ArchetypeAssembliesPrefix)
        )) {
          // For each type in these assemblies
          foreach(Type systemType in assembly.GetExportedTypes().Where(
            // ... abstract types can't be built
            systemType => !systemType.IsAbstract
              // ... if it doesn't have a disqualifying attribute
              && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
              && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
          )) {
            // ... if it extends Archetype<,> 
            if(systemType.IsAssignableToGeneric(typeof(Archetype<,>))) {
              @return.archetypes.Add(systemType);
            } // ... or IModel<>
            else if(systemType.IsAssignableToGeneric(typeof(IModel<>))) {
              // ... if it's an IComponent<>
              if(systemType.IsAssignableToGeneric(typeof(IComponent<>))) {
                @return.components.Add(systemType);
              } else
                @return.models.Add(systemType);
            }
            else
              continue;
          }
        }

        return @return;
      }

      /// <summary>
      /// Try to construct the archetype, which will register it with it's collections:
      /// TODO: change this so if we are missing a dependency archetype, then this tries to load that one, and adds +1 to a depth parameter (default 0) on this function
      /// </summary>
      static void _constructArchetypeFromSystemType(System.Type archetypeSystemType, int depth = 0) {
        // Get ctor
        ConstructorInfo archetypeConstructor = archetypeSystemType.GetConstructor(
          BindingFlags.Instance | BindingFlags.NonPublic,
          null,
          new Type[0],
          null
        );
        object[] ctorArgs = new object[0];

        // We first look for a private parameterless ctor, then for a protected ctor with one argument which inherits from ArchetypeId.
        if(archetypeConstructor == null) {
          archetypeConstructor = archetypeSystemType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(ctor => ctor.GetParameters().Count() == 1 && typeof(Archetype.Identity).IsAssignableFrom(ctor.GetParameters()[0].ParameterType)).FirstOrDefault();
          ctorArgs = new object[] { null };

          if(archetypeConstructor == null) {
            throw new CannotInitializeArchetypeException($"Cannot initialize type: {archetypeSystemType?.FullName ?? "ERRORNULLTYPE"},\n  it does not impliment either:\n\t\t a private or protected parameterless constructor that takes no arguments,\n\t\t or a protected/private ctor that takes one argument that inherits from ArchetypeId that accepts the default of Null for singleton initialization.");
          }
        }

        /// Try to construct it.
        /// The CTor should register it to it's main collection.
        try {
          object archetype = archetypeConstructor.Invoke(ctorArgs);
          // success:
          _initializedArchetypes.Add(archetype as Archetype);
        } // attempt failure: 
        catch(FailedToConfigureNewArchetypeException e) {
          string failureMessage;
          try {
            failureMessage = $"Failed on attempt #{Settings.InitializationAttempts - _remainingInitializationAttempts} to construct new Archetype of type: {archetypeSystemType.FullName} due to unknown internal error. \n ---------- \n Will retry \n ---------- \n.";
          } // fatal:
          catch(Exception ex) {
            string fatalMessage = $"Cannot initialize archetype of type: {archetypeSystemType?.FullName ?? "NULLTYPE"} for unknown reasons. \n ---------- \n Will Not Retry \n ---------- \n.";
            throw new CannotInitializeArchetypeException(fatalMessage, ex);
          }
          throw new FailedToConfigureNewArchetypeException(failureMessage, e);
        } // fatal:
        catch(Exception e) {
          string fatalMessage = $"Cannot initialize archetype of type: {archetypeSystemType?.FullName ?? "NULLTYPE"} Due to unknown inner exception. \n ---------- \n Will Not Retry \n ---------- \n.";
          throw new CannotInitializeArchetypeException(fatalMessage, e);
        }
      }

      /// <summary>
      /// Try to initialize any archetypes that failed:
      /// </summary>
      static void _tryToCompleteInitialization() {
        _uninitializedArchetypes.RemoveAll(archetypeSystemType => {
          try {
            _constructArchetypeFromSystemType(archetypeSystemType);
            return true;
          }
          catch(FailedToConfigureNewArchetypeException) {
            return false;
          }
          catch(CannotInitializeArchetypeException ce) {
            if(Settings.FatalOnCannotInitializeArchetype) {
              throw ce;
            }

            return true;
          }
        });
      }

      /// <summary>
      /// Call all the the Archetype.Modifier.Initialize() functions in mod load order.
      /// </summary>
      static void _applyModifications() {

      }

      /// <summary>
      /// Try to finish all remaining initialized archetypes:
      /// </summary>
      static void _tryToFinishAllInitalizedTypes() {
        _initializedArchetypes.RemoveAll(archetype => {
          try {
            archetype.Finish();

            return true;
          } // attempt failure: 
          catch(FailedToConfigureNewArchetypeException) {
            //Debugger.LogError($"Failed on attempt #{Settings.FinalizationAttempts - RemainingFinalizationAttempts} to construct new Archetype of type: {archetype} due to unknown internal error. \n ---------- \n Will retry \n ---------- \n. \nINTERNAL ERROR: {e}");

            return false;
          }
          catch(CannotInitializeArchetypeException) {
            //Debugger.LogError($"Cannot finish archetype of type: {archetype} due to CannotInitializeArchetypeException. \n ---------- \n Will Not Retry \n ---------- \n INNER EXCEPTION:\n {e}" + $"\n{e}");
            if(Settings.FatalOnCannotInitializeArchetype) {
              throw;
            }

            return true;
          }
          catch(Exception) {
            //Debugger.LogError($"Cannot finish archetype of type: {archetype} Due to unknown inner exception. \n ---------- \n Will Not Retry \n ---------- \n." + $"\n{e}");
            if(Settings.FatalOnCannotInitializeArchetype) {
              throw;
            }

            return true;
          }
        });
      }

      /// <summary>
      /// Finish initialization
      /// </summary>
      static void _finalize() {
        Model.Serializer.Settings.DbContext.SaveChanges();

        _uninitializedArchetypes = null;
        _initializedArchetypes = null;
        _initializedDbContexts = null;
        _uninitializedComponents = null;
        _uninitializedModels = null;

        IsFinished = true;
      }
    }
  }
}