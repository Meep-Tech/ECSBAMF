using Meep.Tech.Data.Utility;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static Meep.Tech.Data.Configuration.Loader.Settings;

namespace Meep.Tech.Data.Configuration {

  /// <summary>
  /// Loads archetypes.
  /// </summary>
  public sealed partial class Loader {

    /// <summary>
    /// The specified settings for this loader
    /// </summary>
    public Settings Options {
      get;
    }

    /// <summary>
    /// The universe this loader creates
    /// </summary>
    public Universe Universe {
      get;
      private set;
    }

    /// <summary>
    /// If all archetypes have been initialized and the loader is finished.
    /// Once this is true, you cannot modify archetypes or their collections anymore.
    /// </summary>
    public bool IsFinished {
      get;
      private set;
    } = false;

    /// <summary>
    /// The assembly types that will be built in order
    /// </summary>
    OrderdDictionary<Assembly, AssemblyBuildableTypesCollection> _assemblyTypesToBuild;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedArchetypes
        = new Dictionary<System.Type, Exception>();
    
    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<System.Type, Exception> _failedArchetypes
        = new Dictionary<System.Type, Exception>();

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedModels
        = new Dictionary<System.Type, Exception>();

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedComponents
        = new Dictionary<System.Type, Exception>();

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    List<Archetype> _initializedArchetypes
        = new List<Archetype>();

    /// <summary>
    /// How many initalization attempts are remaining
    /// </summary>
    int _remainingInitializationAttempts;

    /// <summary>
    /// How many finalization attempts are remaining
    /// </summary>
    int _remainingFinalizationAttempts;

    /// <summary>
    /// Externally fetched assemblies for loading
    /// </summary>
    List<Assembly> _unorderedAssembliesToLoad;

    /// <summary>
    /// The assemblies from Options.PreOrderedAssemblyFiles along with order.json combined and ready to use
    /// </summary>
    public Map<ushort, string> _orderedAssemblyFiles {
      get;
      set;
    } = new Map<ushort, string>();

    /// <summary>
    /// Make a new Archetype Loader.
    /// This can be made to make an Archetype Universe instance.
    /// </summary>
    public Loader(Settings options = null) {
      Options = options ?? new Settings();
      _remainingInitializationAttempts = Options.InitializationAttempts;
      _remainingFinalizationAttempts = Options.FinalizationAttempts;
    }

    /// <summary>
    /// Try to load all archetypes, using the Settings
    /// </summary>
    public void Initialize(Universe universe = null) {
      Universe = universe ?? new Universe(this, Options.UniverseName);
      _initializeModelSerializerSettings();
      _initalizeCompatableArchetypeData();
      _initializeModelsAndArchetypesByAssembly();

      while(_remainingInitializationAttempts-- > 0 && _uninitializedArchetypes.Count > 0) {
        _tryToCompleteAllArchetypesInitialization();
      }

      _applyModificationsToAllTypesByAssemblyLoadOrder();

      while(_remainingFinalizationAttempts-- > 0 && _initializedArchetypes.Count > 0) {
        _tryToFinishAllInitalizedTypes();
      }

      _finalize();
    }

    /// <summary>
    /// Set up initial settings.
    /// </summary>
    void _initalizeCompatableArchetypeData() {
      // pre-load
      Options.PreLoadAssemblies.Count();

      // order assemblies according to the config.json.
      _loadValidAssemblies();
      _loadModLoadOrderFromJson();
      _orderAssembliesByModLoadOrder();

      _loadAllBuildableTypes();
    }

    /// <summary>
    /// Initialize the model serializer
    /// </summary>
    void _initializeModelSerializerSettings() {
      Universe.ModelSerializer = new Model.Serializer();

      // Setup newtonsoft json.net
      /*JsonConvert.DefaultSettings = () => new JsonSerializerSettings() {
        ContractResolver = new DefaultContractResolver() {
          IgnoreSerializableAttribute = false
        }
      };*/

      Universe.ModelSerializer.Options.ComponentJsonSerializerSettings
        ??= new JsonSerializerSettings() {
          ContractResolver = new DefaultContractResolver() {
            IgnoreSerializableAttribute = false,
            NamingStrategy = new CamelCaseNamingStrategy() {
              OverrideSpecifiedNames = false
            }
          }
        };
    }

    /// <summary>
    /// load the dbcontext settings from the info we've gotten from all the models
    /// </summary>
    void _finalizeModelSerializerSettings() {
      Universe.ModelSerializer.Options.DbContext
        ??= Options.GetDefaultDbContextForModelSerialization(
          new DbContextOptions<Model.Serializer.DbContext>(),
          Universe
        );
    }

    /// <summary>
    /// Load all the mods from the mod folder
    /// </summary>
    void _loadModLoadOrderFromJson() {
      _orderedAssemblyFiles = Options.PreOrderedAssemblyFiles;
      string loadOrderFile = Path.Combine(Options.ModsRootFolderLocation, "order.json");
      if(File.Exists(loadOrderFile)) {
        foreach(LoadOrderItem loadOrderItem in JsonConvert.DeserializeObject<List<LoadOrderItem>>(
          File.ReadAllText(loadOrderFile),
          Universe.ModelSerializer.Options.ComponentJsonSerializerSettings
        )) {
          _orderedAssemblyFiles
            .Add(loadOrderItem.Priority, loadOrderItem.AssemblyFileName);
        }
      }
    }

    /// <summary>
    /// Collect all assemblies that could have archetypes into _unorderedAssembliesToLoad
    /// </summary>
    void _loadValidAssemblies() {
      // get built ins.
      List<Assembly> externalAssemblies = new List<Assembly>();
      if(File.Exists(Options.ModsRootFolderLocation)) {
        foreach(string compatableAssemblyFileName in Directory.GetFiles(
          Options.ModsRootFolderLocation,
          $"{Options.ArchetypeAssembliesPrefix}*",
          SearchOption.AllDirectories
        )) {
          externalAssemblies
            .Add(Assembly.LoadFile(compatableAssemblyFileName));
        }
      }

      // load internal assemblies
      _unorderedAssembliesToLoad = externalAssemblies.Concat(AppDomain.CurrentDomain.GetAssemblies())
        // ... that is not dynamic, and that matches any naming requirements
        .Where(assembly => !assembly.IsDynamic
          && assembly.GetName().FullName.StartsWith(Options.ArchetypeAssembliesPrefix)
          && !Options.ArchetypeAssemblyPrefixesToIgnore
            .Where(assemblyPrefix => assembly.GetName().FullName.StartsWith(assemblyPrefix))
            .Any()
      ).ToList();
    }

    /// <summary>
    /// An item for setting up the load order; order.json file.
    /// Used to specify the order to load assemblies in
    /// </summary>
    struct LoadOrderItem {

      /// <summary>
      /// The order in the list/priority.
      /// Lower values go first
      /// </summary>
      public ushort Priority {
        get;
        set;
      }

      /// <summary>
      /// The local file name of the assembly.
      /// Add the folder path if it's in a sub folder too
      /// </summary>
      public string AssemblyFileName {
        get;
        set;
      }
    }

    void _orderAssembliesByModLoadOrder() {
      if(_orderedAssemblyFiles.Any()) {
        Options._assemblyLoadOrder
          = _unorderedAssembliesToLoad.OrderBy(
            assembly => _orderedAssemblyFiles.Reverse
              .TryToGet(assembly.FullName.Split(',')[0], out ushort foundPriority)
              ? foundPriority
              : ushort.MaxValue
        ).ToList();
      } // Random order by default:
      else {
        Options._assemblyLoadOrder
          = _unorderedAssembliesToLoad;
      }
    }

    void _initializeModelsAndArchetypesByAssembly() {
      foreach(AssemblyBuildableTypesCollection typesToBuild in _assemblyTypesToBuild.Values) {

        // components first
        foreach(Type systemType in typesToBuild.Components) {
          try {
            _registerComponentType(systemType);
          }
          /*catch(MissingComponentDependencyException de) {
            _uninitializedComponents.Add(systemType, de);
          }
          catch(Exception ex) {
            throw new CannotInitializeModelException($"Could not initialize Component of type {systemType} due to Unknown Inner Exception.", ex);
          }*/
          catch(Exception de) {
            _uninitializedComponents.Add(systemType, de);
          }
        }

        // then initialize archetypes:
        foreach(Type systemType in typesToBuild.Archetypes) {
          _tryToInitializeArchetype(systemType);
        }

        // then register models
        foreach(Type systemType in typesToBuild.Models) {
          try {
            _registerModelType(systemType);
          }
          /*catch(MissingComponentDependencyException de) {
            _uninitializedModels.Add(systemType, de);
          }
          catch(Exception ex) {
            throw new CannotInitializeModelException($"Could not initialize Model of type {systemType} due to Unknown Inner Exception.", ex);
          }*/
          catch(Exception de) {
            _uninitializedModels.Add(systemType, de);
          }
        }
      }
    }

    /// <summary>
    /// Register a new type of model.
    /// </summary>
    void _registerModelType(Type systemType) {

      // invoke static ctor
      System.Runtime.CompilerServices
        .RuntimeHelpers
        .RunClassConstructor(systemType.TypeHandle);

      systemType.GetMethod(
        nameof(IModel.Setup),
        BindingFlags.Instance
          | BindingFlags.NonPublic
          | BindingFlags.Static
      )?.Invoke(null, new object[] { Universe });

      // assign root archetype references
      if(systemType.IsAssignableToGeneric(typeof(IModel<,>))) {
        var types = systemType.GetInheritedGenericTypes(typeof(IModel<,>));
        if(!typeof(IModel.IBuilderFactory).IsAssignableFrom(types.Last())) {
          Universe.Archetypes._rootArchetypeTypesByBaseModelType[systemType.FullName] 
            = types.Last();
        }
      }

      _testBuildDefaultModel(systemType);
      try {
        Universe.Models._baseTypes.Add(
          systemType.FullName,
          systemType.GetInheritedGenericTypes(typeof(IModel<>)).FirstOrDefault()
            ?? systemType.GetInheritedGenericTypes(typeof(IModel<,>)).First()
        );
      } catch(Exception e) {
        throw new NotImplementedException($"Could not find IModel<> Base Type for {systemType}, does it inherit from IModel instead of IModel<T> by mistake?", e);
      }
      if(!Universe.ModelSerializer.Options.TypesToMapToDbContext.ContainsKey(systemType)) {
        Universe.ModelSerializer.Options.TypesToMapToDbContext[systemType] = null;
      }
    }

    /// <summary>
    /// Register types of components
    /// </summary>
    void _registerComponentType(Type systemType) {

      // invoke static ctor
      System.Runtime.CompilerServices
        .RuntimeHelpers
        .RunClassConstructor(systemType.TypeHandle);

      systemType.GetMethod(
        nameof(IModel.Setup),
        BindingFlags.Instance
          | BindingFlags.NonPublic
          | BindingFlags.Static
      )?.Invoke(null, new object[] { Universe });

      _testBuildDefaultComponent(systemType);
      try {
        Universe.Components._baseTypes.Add(
          systemType.FullName,
          systemType.GetInheritedGenericTypes(typeof(IComponent<>)).First()
        );
      } catch (Exception e) {
        throw new NotImplementedException($"Could not find IComponent<> Base Type for {systemType}, does it inherit from IComponent instead of IComponent<T> by mistake?", e);
      }
    }

    /// <summary>
    /// Test build a model of the given type using it's default archetype or builder.
    /// </summary>
    void _testBuildDefaultModel(Type systemType) {
      Archetype defaultFactory;
      if(systemType.IsAssignableToGeneric(typeof(Model<,>))) {
        defaultFactory 
          = Universe.Archetypes.GetDefaultForModelOfType(systemType);
      }
      else {
        defaultFactory = Universe.Models.GetBuilderFactoryFor(systemType) as Archetype;
      }

      if(defaultFactory == null) {
        throw new Exception($"Could not make a default model for model of type: {systemType.FullName}. Could not fine a default BuilderFactory or Archetype to build it with.");
      }

      try {
        Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor
          = defaultFactory.GetGenericBuilderConstructor();
        IBuilder builder = builderCtor.Invoke(
          defaultFactory,
          defaultFactory.DefaultTestParams
        );
        IModel defaultModel = defaultFactory.MakeDefaultWith(builder);
        Universe.Models._modelTypesProducedByArchetypes[defaultFactory] = defaultModel.GetType();
      }
      catch (Exception e) {
        throw new Exception($"Could not make a default model for model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}.", e);
      }
    }

    /// <summary>
    /// Test build a model of the given type using it's default archetype or builder.
    /// </summary>
    void _testBuildDefaultComponent(Type systemType) {
      if(!(Universe.Components.GetBuilderFactoryFor(systemType) is Archetype defaultFactory)) {
        throw new Exception($"Could not make a default component model of type: {systemType.FullName}. Could not fine a default BuilderFactory to build it with.");
      }

      try {
        Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor 
          = defaultFactory.GetGenericBuilderConstructor();
        IBuilder builder = builderCtor.Invoke(
          defaultFactory,
          defaultFactory.DefaultTestParams
        );
        defaultFactory.MakeDefaultWith(builder);

        /// Register component key
        Universe.Components.
          _byKey[(defaultFactory as IComponent.IBuilderFactory).Key] 
            = systemType;
      }
      catch (Exception e) {
        throw new Exception($"Could not make a default component model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}.", e);
      }
    }

    class AssemblyBuildableTypesCollection {

      internal List<Type> Archetypes
          = new List<Type>();
      internal List<Type> Models
          = new List<Type>();
      internal List<Type> Components
          = new List<Type>();
      internal Type Modifications;
      internal Assembly Assembly {
        get;
      }

      public AssemblyBuildableTypesCollection(Assembly assembly) {
        Assembly = assembly;
      }
    }

    /// <summary>
    /// Get all types that this loader knows how to build from the loaded assemblies.
    /// Sets _assemblyTypesToBuild
    /// </summary>
    void _loadAllBuildableTypes() {
      _assemblyTypesToBuild =
        new OrderdDictionary<Assembly, AssemblyBuildableTypesCollection>();

      // TODO: allow the assemblies to somehow apply a load order.
      // Maybe provide their own weight, or an ini/json file with weights/settings
      // after that we should also get the Archetype.Modifier classes from each assembly if they exist.

      // For each loaded assembly
      foreach(Assembly assembly in Options.AssemblyLoadOrder) {
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
            _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
            _assemblyTypesToBuild[assembly].Archetypes.Add(systemType);
          } // ... or IModel<>
          else if(systemType.IsAssignableToGeneric(typeof(IModel<>))) {
            _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
            // ... if it's an IComponent<>
            if(systemType.IsAssignableToGeneric(typeof(IComponent<>))) {
              _assemblyTypesToBuild[assembly].Components.Add(systemType);
            }
            else
              _assemblyTypesToBuild[assembly].Models.Add(systemType);
          } // if it's a modifications class:
          else if(typeof(Modifications).IsAssignableFrom(systemType)) {
            _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
            _assemblyTypesToBuild[assembly].Modifications = systemType;
          }
          else
            continue;
        }
      }
    }

    /// <summary>
    /// Make sure the assembly collection exists for the given assmebly. Add a new one if it doesnt.
    /// </summary>
    static void _validateAssemblyCollectionExists(
      OrderdDictionary<Assembly, AssemblyBuildableTypesCollection> allCollections,
      Assembly assembly
    ) {
      if(!allCollections.Contains(assembly)) {
        allCollections.Add(assembly, new AssemblyBuildableTypesCollection(assembly));
      }
    }

    /// <summary>
    /// Try to construct the archetype, which will register it with it's collections:
    /// TODO: change this so if we are missing a dependency archetype, then this tries to load that one by name, and then adds +1 to a depth parameter (default 0) on this function.
    /// Maybe this could be done more smoothly by pre-emptively registering all ids?
    /// </summary>
    void _constructArchetypeFromSystemType(System.Type archetypeSystemType, int depth = 0) {
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
      Archetype archetype;
      try {
         archetype = (Archetype)archetypeConstructor.Invoke(ctorArgs);

        // Try to make the default model, and register what that is:
        Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor
          = archetype.GetGenericBuilderConstructor();

        IBuilder builder = builderCtor.Invoke(
          archetype,
          archetype.DefaultTestParams
        );

        // load branch attribute if there is one
        BranchAttribute branchAttribute;
        // (first one is newest inherited)
        if((branchAttribute = archetypeSystemType.GetCustomAttributes<BranchAttribute>().FirstOrDefault()) != null) {
          // TODO: faster cache for this?
          (archetype as IFactory).ModelConstructor = builder
            => (IModel)Activator.CreateInstance(
              branchAttribute.NewBaseModelType 
                // Defaults to decalring type (surrounding type) if one wasn't specified.
                ??= GetFirstDeclaringParent(archetypeSystemType),
              // nonpublic ctors allowed
              true
            );
        }

        IModel defaultModel = archetype.MakeDefaultWith(builder);
        if(!Universe.Archetypes._rootArchetypeTypesByBaseModelType.ContainsKey(defaultModel.GetType().FullName)) {
          Universe.Archetypes._rootArchetypeTypesByBaseModelType[defaultModel.GetType().FullName] = archetype.GetType();
        }

        // success:
        _initializedArchetypes.Add(archetype);
      } // attempt failure: 
      catch(FailedToConfigureNewArchetypeException e) {
        string failureMessage;
        try {
          failureMessage = $"Failed on attempt #{Options.InitializationAttempts - _remainingInitializationAttempts} to construct new Archetype of type: {archetypeSystemType.FullName} due to unknown internal error. \n ---------- \n Will retry \n ---------- \n.";
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
    /// Go up the tree and find a declaring type that these types inherit from.
    /// </summary>
    static Type GetFirstDeclaringParent(Type archetypeSystemType) {
      if(archetypeSystemType.DeclaringType == null) {
        if(archetypeSystemType.BaseType != null) {
          return GetFirstDeclaringParent(archetypeSystemType.BaseType);
        }
        else
          return null;
      }
      else
        return archetypeSystemType.DeclaringType;
    }

    /// <summary>
    /// Try to initialize any archetypes that failed:
    /// </summary>
    void _tryToCompleteAllArchetypesInitialization() {
      _uninitializedArchetypes.Keys.ToList().ForEach(archetypeSystemType => {
        _tryToInitializeArchetype(archetypeSystemType);
      });
    }

    void _tryToInitializeArchetype(Type archetypeSystemType) {
      try {
        _constructArchetypeFromSystemType(archetypeSystemType);
        _uninitializedArchetypes.Remove(archetypeSystemType);
      }
      catch(FailedToConfigureNewArchetypeException fe) {
        _uninitializedArchetypes.Add(archetypeSystemType, fe);
      }
      catch(CannotInitializeArchetypeException ce) {
        if(Options.FatalOnCannotInitializeType) {
          throw ce;
        }

        _failedArchetypes.Add(archetypeSystemType, ce);
        _uninitializedArchetypes.Remove(archetypeSystemType);
      }
    }

    /// <summary>
    /// Call all the the Archetype.Modifier.Initialize() functions in mod load order.
    /// </summary>
    void _applyModificationsToAllTypesByAssemblyLoadOrder() {
      foreach(System.Type modifierType in _assemblyTypesToBuild.Values
        .Select(a => a.Modifications)
        .Where(v => !(v is null))
      ) {
        Modifications modifier
            = Activator.CreateInstance(modifierType, this)
              as Modifications;

        modifier.Initialize();
      }
    }

    /// <summary>
    /// Try to finish all remaining initialized archetypes:
    /// </summary>
    void _tryToFinishAllInitalizedTypes() {
      _initializedArchetypes.RemoveAll(archetype => {
        try {
          archetype.Finish();

          return true;
        } // attempt failure: 
        catch(FailedToConfigureNewArchetypeException) {
          //Debugger.LogError($"Failed on attempt #{Options.FinalizationAttempts - RemainingFinalizationAttempts} to construct new Archetype of type: {archetype} due to unknown internal error. \n ---------- \n Will retry \n ---------- \n. \nINTERNAL ERROR: {e}");

          return false;
        }
        catch(CannotInitializeArchetypeException) {
          //Debugger.LogError($"Cannot finish archetype of type: {archetype} due to CannotInitializeArchetypeException. \n ---------- \n Will Not Retry \n ---------- \n INNER EXCEPTION:\n {e}" + $"\n{e}");
          if(Options.FatalOnCannotInitializeType) {
            throw;
          }

          return true;
        }
        catch(Exception) {
          //Debugger.LogError($"Cannot finish archetype of type: {archetype} Due to unknown inner exception. \n ---------- \n Will Not Retry \n ---------- \n." + $"\n{e}");
          if(Options.FatalOnCannotInitializeType) {
            throw;
          }

          return true;
        }
      });
    }

    /// <summary>
    /// Finish initialization
    /// </summary>
    void _finalize() {
      _reportOnFailedTypeInitializations();
      _finalizeModelSerializerSettings();
      Universe.ModelSerializer.Options.DbContext.SaveChanges();

      _uninitializedArchetypes = null;
      _initializedArchetypes = null;
      _uninitializedComponents = null;
      _uninitializedModels = null;
      _assemblyTypesToBuild = null;
      _unorderedAssembliesToLoad = null;
      _orderedAssemblyFiles = null;
      _remainingFinalizationAttempts = Options.FinalizationAttempts;
      _remainingInitializationAttempts = Options.InitializationAttempts;

      IsFinished = true;
    }

    void _reportOnFailedTypeInitializations() {
      bool failureDetected = false;
      foreach((System.Type componentType, Exception ex) in _uninitializedModels) {
        Console.Error.WriteLine($"Could not initialize Model Type: {componentType}, due to Internal Exception:\n\n{ex}");
        failureDetected = true;
      }
      foreach((System.Type modelType, Exception ex) in _uninitializedComponents) {
        Console.Error.WriteLine($"Could not initialize Component Type: {modelType}, due to Internal Exception:\n\n{ex}");
        failureDetected = true;
      }
      foreach((System.Type archetypeType, Exception ex) in _uninitializedArchetypes.Merge(_failedArchetypes)) {
        Console.Error.WriteLine($"Could not initialize Archetype Type: {archetypeType}, due to Internal Exception:\n\n{ex}");
        failureDetected = true;
      }

      if(Options.FatalOnCannotInitializeType && failureDetected) {
        throw new InvalidOperationException("Failed to initialize several types in the Archetype Loader. See full Error Stream Logs for details.");
      }
    }
  }
}