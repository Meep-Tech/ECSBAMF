using Meep.Tech.Collections.Generic;
using Meep.Tech.Data.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using static Meep.Tech.Data.Archetype;

namespace Meep.Tech.Data.Configuration {

  /// <summary>
  /// Loads archetypes.
  /// </summary>
  public sealed partial class Loader {
    internal Dictionary<System.Type, Dictionary<System.Type, IModel>> _testModels;
    Dictionary<Archetype, System.Type> _loadedTestParams;
    List<System.Type> _initializedTypes;
    List<Assembly> _assemblyLoadOrder;

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
      internal set;
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
    /// Types that failed to initialize and their exceptions.
    /// </summary>
    public IReadOnlyList<Failure> Failures {
      get;
      private set;
    }

    /// <summary>
    /// Assembled mod load order.
    /// </summary>
    public IReadOnlyList<Assembly> AssemblyLoadOrder
      => _assemblyLoadOrder;

    /// <summary>
    /// Types that failed to initialize and their exceptions.
    /// </summary>
    public IEnumerable<System.Type> InitializedTypes
      => _initializedTypes;

    /// <summary>
    /// The assembly types that will be built in order
    /// </summary>
    OrderedDictionary<Assembly, AssemblyBuildableTypesCollection> _assemblyTypesToBuild;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedArchetypes;

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<System.Type, Exception> _failedArchetypes;

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<MemberInfo, Exception> _failedEnumerations;

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<System.Type, Exception> _failedModels;

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<System.Type, Exception> _failedComponents;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedModels;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedComponents;

    /// <summary>
    /// The types that have been constructed and still need model data mapped to them.
    /// </summary>
    internal HashSet<Archetype> _initializedArchetypes;

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
    Map<ushort, string> _orderedAssemblyFiles;

    #region Initialization

    /// <summary>
    /// Make a new Archetype Loader.
    /// This can be made to make an Archetype Universe instance.
    /// </summary>
    public Loader(Settings options = null) {
      Options = options ?? new Settings();
    }

    /// <summary>
    /// Try to load all archetypes, using the Settings
    /// </summary>
    public void Initialize(Universe universe = null) {
      _initFields();

      Universe = universe ?? Universe ?? new Universe(this, Options.UniverseName);
      Universe._extraContexts.ToList().ForEach(context => context.Value.OnLoaderInitialize());
      _initializeModelSerializerSettings();
      _initalizeCompatableArchetypeData();
      _initializeTypesByAssembly();

      while (_remainingInitializationAttempts-- > 0 && _uninitializedArchetypes.Count + _uninitializedModels.Count + _uninitializedComponents.Count > 0) {
        _tryToCompleteAllModelsInitialization();
        _tryToCompleteAllArchetypesInitialization();
        _tryToCompleteAllComponentsInitialization();
      }

      _testBuildModelsForAllInitializedTypes();

      Universe._extraContexts.ToList().ForEach(context => context.Value.OnAllTypesInitializationComplete());

      _applyModificationsToAllTypesByAssemblyLoadOrder();

      Universe._extraContexts.ToList().ForEach(context => context.Value.OnModificationsComplete());

      while (_remainingFinalizationAttempts-- > 0 && _initializedArchetypes.Count > 0) {
        _tryToFinishAllInitalizedTypes();
      }

      _finalize();
    }

    void _initFields() {
      _remainingInitializationAttempts = Options.InitializationAttempts;
      _remainingFinalizationAttempts = Options.FinalizationAttempts;

      _testModels = new();
      _loadedTestParams = new();
      _orderedAssemblyFiles = new();
      _assemblyLoadOrder = new();
      _unorderedAssembliesToLoad = new();

      _initializedTypes = new();
      _initializedArchetypes = new();

      _uninitializedComponents = new();
      _uninitializedModels = new();
      _uninitializedArchetypes = new();

      _failedEnumerations = new();
      _failedArchetypes = new();
      _failedComponents = new();
      _failedModels = new();
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
      Universe.ModelSerializer
        = new Model.Serializer(Options.ModelSerializerOptions, Universe);
    }

    void _initializeTypesByAssembly() {
      foreach (AssemblyBuildableTypesCollection typesToBuild in _assemblyTypesToBuild.Values) {
        // enums first:
        foreach (PropertyInfo prop in typesToBuild.Enumerations) {
          _registerEnumValue(prop);
        }

        // components next: 
        foreach (Type systemType in typesToBuild.Components) {
          if (!_tryToInitializeComponent(systemType, out var e)) {
            if (e is CannotInitializeTypeException) {
              _failedComponents[systemType] = e;
            } else
              _uninitializedComponents[systemType] = e;
          }
        }

        // then we run the static initializers for all simple models:
        foreach (Type systemType in typesToBuild.Models.Where(t => typeof(IModel<>).IsAssignableFrom(t))) {
          if (!_tryToPreInitializeSimpleModel(systemType, out var e)) {
            if (e is CannotInitializeTypeException) {
              _failedModels[systemType] = e;
            }
            else
              _uninitializedModels[systemType] = e;
          }
        }

        // then initialize archetypes:
        foreach (Type systemType in typesToBuild.Archetypes) {
          if (!_tryToInitializeArchetype(systemType, out var e)) {
            if (e is CannotInitializeTypeException) {
              _failedArchetypes[systemType] = e;
            } else
              _uninitializedArchetypes[systemType] = e;
          }
        }

        // then register models
        foreach (Type systemType in typesToBuild.Models.Except(_failedModels.Keys)) {
          if (!_tryToInitializeModel(systemType, out var e)) {
            if (e is CannotInitializeTypeException) {
              _failedModels[systemType] = e;
            } else
              _uninitializedModels[systemType] = e;
          }
        }
      }
    }

    void _runStaticCtorsFromBaseClassUp(System.Type @class) {
      List<System.Type> newAncestors = new() {
        @class
      };
      while ((@class = @class.BaseType) != null) {
        if (_staticallyInitializedTypes.Contains(@class)) {
          break;
        }
        else {
          _staticallyInitializedTypes.Add(@class);
          newAncestors.Add(@class);
        }
      }

      newAncestors.Reverse();
      foreach (System.Type type in newAncestors) {
        try {
          // invoke static ctor
          System.Runtime.CompilerServices
            .RuntimeHelpers
            .RunClassConstructor(type.TypeHandle);
        }
        catch (Exception e) {
          throw new Exception($"Failed to run static constructor for ancestor: {type?.FullName ?? "null"}, of type: {@class.FullName}.\n=Exception:{e}\n\n=Inner Exception\n{e.InnerException}");
        }
      }
    }

    #region Assembly Load Order Init

    /// <summary>
    /// Load all the mods from the mod folder
    /// </summary>
    void _loadModLoadOrderFromJson() {
      _orderedAssemblyFiles = Options.PreOrderedAssemblyFiles;
      string loadOrderFile = Path.Combine(Options.DataFolderParentFolderLocation, "order.json");
      if (File.Exists(loadOrderFile)) {
        foreach (LoadOrderItem loadOrderItem
          in JsonConvert.DeserializeObject<List<LoadOrderItem>>(
           File.ReadAllText(loadOrderFile))
        ) {
          _orderedAssemblyFiles
            .Add(loadOrderItem.Priority, loadOrderItem.AssemblyFileName);
        }
      }
    }

    void _orderAssembliesByModLoadOrder() {
      if (_orderedAssemblyFiles.Forward.Any()) {
        _assemblyLoadOrder
          = _unorderedAssembliesToLoad.OrderBy(
            assembly => _orderedAssemblyFiles.Reverse
              .TryGetValue(assembly.FullName.Split(',')[0], out ushort foundPriority)
                ? foundPriority
                : ushort.MaxValue
        ).ToList();
      } // Random order by default:
      else {
        _assemblyLoadOrder
          = _unorderedAssembliesToLoad;
      }
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

    #endregion

    #region Assembly and System Type Init

    /// <summary>
    /// Collect all assemblies that could have archetypes into _unorderedAssembliesToLoad
    /// </summary>
    void _loadValidAssemblies() {
      // load internal assemblies
      List<Assembly> defaultAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
      if (Options.PreLoadAllReferencedAssemblies) {
        var moreAssemblies = defaultAssemblies.SelectMany(
          a => a.GetReferencedAssemblies()
            .Where(assembly => _validateAssemblyByName(assembly))
            .Select((item) => {
              try {
                return Assembly.Load(item);
              }
              catch (Exception e) {
                throw new Exception($"Could not load assembly:{item.FullName}", e);
              }
            })).ToList();

        defaultAssemblies.AddRange(moreAssemblies);
      }

      // get ones added to the load order.
      List<Assembly> externalAssemblies = new();
      var externalAssemblyLocations = Options.PreOrderedAssemblyFiles.Forward.Values.Except(
          defaultAssemblies.Where(a => !a.IsDynamic).Select(a => a.Location)
      );
      if (externalAssemblyLocations.Any()) {
        foreach (var compatableAssemblyFileName in externalAssemblyLocations) {
          externalAssemblies
            .Add(Assembly.LoadFrom(compatableAssemblyFileName));
        }
      }

      // combine and filter them
      _unorderedAssembliesToLoad = defaultAssemblies.Concat(externalAssemblies)
        .Except(Options.IgnoredAssemblies)
        // ... that is not dynamic, and that matches any naming requirements
        .Where(assembly => !assembly.IsDynamic
          && (Options.PreLoadAssemblies.Contains(assembly)
            || _validateAssemblyByName(assembly.GetName()))
        ).ToHashSet().ToList();
    }

    bool _validateAssemblyByName(AssemblyName assembly) 
      => assembly.FullName.StartsWith(Options.ArchetypeAssembliesPrefix)
        && !Options.AssemblyPrefixesToIgnore
          .Where(assemblyPrefix => assembly.FullName.StartsWith(assemblyPrefix))
          .Any();

    /// <summary>
    /// Get all types that this loader knows how to build from the loaded assemblies.
    /// Sets _assemblyTypesToBuild
    /// </summary>
    void _loadAllBuildableTypes() {
      _assemblyTypesToBuild =
        new OrderedDictionary<Assembly, AssemblyBuildableTypesCollection>();

      // TODO: allow the assemblies to somehow apply a load order.
      // Maybe provide their own weight, or an ini/json file with weights/settings
      // after that we should also get the Archetype.Modifier classes from each assembly if they exist.

      // For each loaded assembly
      foreach (Assembly assembly in AssemblyLoadOrder) {
        // For each type in these assemblies
        foreach (Type systemType in assembly.GetExportedTypes().Where(
          // ... abstract types can't be built
          systemType =>
            // ... if it doesn't have a disqualifying attribute
            (!Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
              && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildThisOrChildrenInInitialLoadAttribute), true))
            || typeof(ISplayed).IsAssignableFrom(systemType)
        )) {
          if (!systemType.IsAbstract) {
            // ... if it extends Archetype<,> 
            if (systemType.IsAssignableToGeneric(typeof(Archetype<,>))) {
              _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
              _assemblyTypesToBuild[assembly].Archetypes.Add(systemType);
              IEnumerable<DependencyAttribute> dependencies = systemType.GetCustomAttributes<DependencyAttribute>();
              if (dependencies?.Any() ?? false) {
                Universe.Archetypes._dependencies[systemType] = dependencies.Select(d => d.DependentOnType);
              }
            } // ... or IModel<>
            else if (systemType.IsAssignableToGeneric(typeof(IModel<>))) {
              _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
              // ... if it's an IComponent<>
              if (systemType.IsAssignableToGeneric(typeof(IComponent<>))) {
                _assemblyTypesToBuild[assembly].Components.Add(systemType);
                IEnumerable<DependencyAttribute> dependencies = systemType.GetCustomAttributes<DependencyAttribute>();
                if (dependencies?.Any() ?? false) {
                  Universe.Components._dependencies[systemType] = dependencies.Select(d => d.DependentOnType);
                }
              }
              else {
                _assemblyTypesToBuild[assembly].Models.Add(systemType);
                IEnumerable<DependencyAttribute> dependencies = systemType.GetCustomAttributes<DependencyAttribute>();
                if (dependencies?.Any() ?? false) {
                  Universe.Models._dependencies[systemType] = dependencies.Select(d => d.DependentOnType);
                }
              }
            } // if it's a modifications class:
            else if (typeof(Modifications).IsAssignableFrom(systemType)) {
              _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
              _assemblyTypesToBuild[assembly].Modifications = systemType;
              //TOOD: modifications file can have dependencies on other modifications classes. This makes the whole assembly wait for the dependent one!.
            }
          }

          // if this type's got enums we want:
          if (systemType.GetCustomAttribute<BuildAllDeclaredEnumValuesOnInitialLoadAttribute>() != null
            || systemType.IsAssignableToGeneric(typeof(Enumeration<>))
            || (systemType.IsAssignableToGeneric(typeof(Archetype<,>)) 
              && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
                && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildThisOrChildrenInInitialLoadAttribute), true))
          ) {
            _validateAssemblyCollectionExists(_assemblyTypesToBuild, assembly);
            foreach (PropertyInfo staticEnumProperty in systemType.GetProperties(BindingFlags.Static | BindingFlags.Public).Where(p => p.PropertyType.IsAssignableToGeneric(typeof(Enumeration<>)))) {
              _assemblyTypesToBuild[assembly].Enumerations.Add(staticEnumProperty);
            }
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
      OrderedDictionary<Assembly, AssemblyBuildableTypesCollection> allCollections,
      Assembly assembly
    ) {
      if (!allCollections.ContainsKey(assembly)) {
        allCollections.Add(assembly, new AssemblyBuildableTypesCollection(assembly));
      }
    }

    class AssemblyBuildableTypesCollection {

      internal List<Type> Archetypes
          = new();
      internal List<Type> Models
          = new();
      internal List<Type> Components
          = new();
      internal List<PropertyInfo> Enumerations
          = new();
      internal Type Modifications;

      internal Assembly Assembly {
        get;
      }

      public AssemblyBuildableTypesCollection(Assembly assembly) {
        Assembly = assembly;
      }
    }

    #endregion

    #region Archetype Init

    bool _tryToInitializeArchetype(Type systemType, out Exception e) {
      /// Check dependencies. TODO: for a,m,&c, index items by their waiting dependency and when a type loads check if anything is waiting on it, and try to load it then.
      if (Universe.Archetypes.Dependencies.TryGetValue(systemType, out var dependencies)) {
        Type firstUnloadedDependency
          = dependencies.FirstOrDefault(t => !_initializedTypes.Contains(t));
        if (firstUnloadedDependency != null) {
          e = new MissingDependencyForArchetypeException(systemType, firstUnloadedDependency);
          return false;
        }
      }

      bool isSplayed;
      Archetype archetype = null;
      try {
        isSplayed = typeof(ISplayed).IsAssignableFrom(systemType);
        // if not we need to construct a new one
        if (!isSplayed 
          || (!Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
            && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildThisOrChildrenInInitialLoadAttribute), true))
        ) {
          archetype = _constructArchetypeFromSystemType(systemType);
          _uninitializedArchetypes.Remove(systemType);
        }
      } catch (CannotInitializeArchetypeException ce) {
        if (Options.FatalOnCannotInitializeType) {
          throw ce;
        }

        e = ce;
        return false;
      } catch (FailedToConfigureNewArchetypeException fe) {
        e = fe;
        return false;
      }

      // init splayed
      if (isSplayed) {
        _initializeSplayedArchetype(systemType);
      }

      // done initializing!
      _initializedTypes.Add(systemType);
      if (archetype is not null) {
        Universe._extraContexts
          .ForEach(context => context.Value.OnArchetypeWasInitialized(systemType, archetype));
      }

      e = null;
      return true;
    }

    void _initializeSplayedArchetype(Type systemType) {
      object dummy;
      List<GenericTestArgumentAttribute> attributes = new();
      if (systemType.ContainsGenericParameters) {
        if ((attributes = systemType.GetCustomAttributes().Where(a => a is GenericTestArgumentAttribute).Cast<GenericTestArgumentAttribute>().ToList()).Any()) {
          dummy = FormatterServices.GetUninitializedObject(systemType.MakeGenericType(attributes.OrderBy(a => a.Order).Select(a => a.GenericArgumentType).ToArray()));
        } else throw new InvalidDataException($"Splayed Archetype of type: {systemType.Name} is generic, and does not implement one of more of the GenericTestArgumentAttribute");
      } else {
        dummy = FormatterServices.GetUninitializedObject(systemType);
      }

      foreach (var splayType in systemType.GetAllInheritedGenericTypes(typeof(Archetype.IBuildOneForEach<,>))) {
        if (splayType.GetGenericArguments().Last() == systemType) {
          var getMethod = _getSplayerArchetypeCtor(dummy, splayType);
          _constructSplayedArchetypes(systemType, splayType, getMethod);
        }
      }

      foreach (var splayType in systemType.GetAllInheritedGenericTypes(typeof(Archetype.IBuildOneForEach<,>.Lazily))) {
        if (splayType.GetGenericArguments().Last() == systemType) {
          var getMethod = _getSplayerArchetypeCtor(dummy, splayType);
          _prepareLazilySplayedArchetype(splayType, getMethod);
        }
      }
    }

    static Func<Enumeration, Archetype> _getSplayerArchetypeCtor(object dummy, Type splayType) {
      MethodInfo getMethodInfo
        = splayType.GetMethod("ConstructArchetypeFor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
       Func<Enumeration, Archetype> getMethod = new(@enum => (Archetype)getMethodInfo.Invoke(dummy, new[] { @enum }));
      return getMethod;
    }

    void _constructSplayedArchetypes(Type systemType, Type splayType,  Func<Enumeration, Archetype> getMethod) {
      foreach (var @enum in Universe.Enumerations.GetAllByType(splayType.GetGenericArguments().First())) {
        var newType = getMethod(@enum);
        Universe._extraContexts
          .ForEach(context => context.Value.OnArchetypeWasInitialized(systemType, newType));
      }
    }

    static void _prepareLazilySplayedArchetype(Type splayType,  Func<Enumeration, Archetype> getMethod) {
      System.Type enumType = splayType.GetGenericArguments()[0];
      System.Type enumBaseType = enumType.GetFirstInheritedGenericTypeParameters(typeof(Enumeration<>)).First();
      if(ISplayedLazily._lazySplayedArchetypesByEnumBaseTypeAndEnumType.TryGetValue(enumBaseType, out var existingWaitingLazySplayedTypes)) {
        if (existingWaitingLazySplayedTypes.TryGetValue(enumType, out var existingWaitingLazyCtors)) {
          existingWaitingLazyCtors.Add(getMethod);
        } else existingWaitingLazySplayedTypes.Add(enumType, new() { getMethod });
      } else {
        ISplayedLazily._lazySplayedArchetypesByEnumBaseTypeAndEnumType[enumBaseType] = new() {
          {enumType, new() {getMethod } }
        };
      }
    }

    /// <summary>
    /// Try to construct the archetype, which will register it with it's collections:
    /// TODO: change this so if we are missing a dependency archetype, then this tries to load that one by name, and then adds +1 to a depth parameter (default 0) on this function.
    /// Maybe this could be done more smoothly by pre-emptively registering all ids?
    /// </summary>
    Archetype _constructArchetypeFromSystemType(System.Type archetypeSystemType, int depth = 0) {
      // see if we have a partially initialized archetype registered
      Archetype archetype = archetypeSystemType?.TryToGetAsArchetype();

      /// Try to construct it.
      /// The CTor should register it to it's main collection.
      try {
        if (archetype is null) {
          // Get ctor
          _getArchetypeConstructorAndArgs(archetypeSystemType, out ConstructorInfo archetypeConstructor, out object[] ctorArgs);
          archetype = (Archetype)archetypeConstructor.Invoke(ctorArgs);
        }

        // success:
        _initializedArchetypes.Add(archetype);
      } // attempt failure: 
      catch (FailedToConfigureTypeException e) {
        string failureMessage = $"Failed on attempt #{Options.InitializationAttempts - _remainingInitializationAttempts} to construct new Archetype of type: {archetypeSystemType.FullName} due to unknown internal error. \n ---------- \n Will retry \n ---------- \n.";
        throw new FailedToConfigureNewArchetypeException(failureMessage, e);
      } // fatal:
      catch (Exception e) {
        string fatalMessage = $"Cannot initialize archetype of type: {archetypeSystemType?.FullName ?? "NULLTYPE"} Due to unknown inner exception. \n ---------- \n Will Not Retry \n ---------- \n.";
        throw new CannotInitializeArchetypeException(fatalMessage, e);
      }

      return archetype;
    }

    static void _getArchetypeConstructorAndArgs(Type archetypeSystemType, out ConstructorInfo archetypeConstructor, out object[] ctorArgs) {
      archetypeConstructor = archetypeSystemType.GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic,
        null,
        new Type[0],
        null
      );
      ctorArgs = new object[0];

      // We first look for a private parameterless ctor, then for a protected ctor with one argument which inherits from ArchetypeId.
      if (archetypeConstructor == null) {
        archetypeConstructor = archetypeSystemType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
          .Where(ctor => ctor.GetParameters().Count() == 1 && typeof(Archetype.Identity).IsAssignableFrom(ctor.GetParameters()[0].ParameterType)).FirstOrDefault();
        ctorArgs = new object[] { null };

        if (archetypeConstructor == null) {
          throw new CannotInitializeArchetypeException($"Cannot initialize type: {archetypeSystemType?.FullName ?? "ERRORNULLTYPE"},\n  it does not impliment either:\n\t\t a private or protected parameterless constructor that takes no arguments,\n\t\t or a protected/private ctor that takes one argument that inherits from ArchetypeId that accepts the default of Null for singleton initialization.");
        }
      }
    }

    /// <summary>
    /// Try to build a test model for each archetype, and throw if it fails
    /// </summary>
    void _testBuildModelsForAllInitializedTypes() {
      foreach (var archetype in _initializedArchetypes) {
        if(!_tryToBuildDefaultModelForArchetype(archetype.GetType(), archetype, out var failureException)) {
          _initializedArchetypes.Remove(archetype);
          _uninitializedArchetypes.Add(archetype.GetType(), failureException);
          _failedArchetypes.Add(archetype.GetType(), failureException);
          archetype.TryToUnload();
        }
      }
    }

    bool _tryToBuildDefaultModelForArchetype(Type archetypeSystemType, Archetype archetype, out Exception exception) {
      exception = null;
      IModel defaultModel = null;
      try {
        // load branch attribute and set the new model ctor if there is one
        BranchAttribute branchAttribute;
        // (first one is newest inherited)
        if ((branchAttribute = archetypeSystemType.GetCustomAttributes<BranchAttribute>().FirstOrDefault()) != null) {
          branchAttribute.NewBaseModelType
            // Defaults to decalring type (surrounding type) if one wasn't specified.
            ??= _getFirstDeclaringParent(archetypeSystemType);

          // set it to the produced type for now
          archetype.ModelTypeProduced = branchAttribute.NewBaseModelType;

          Func<IBuilder, IModel> defaultModelConstructor
            = Universe.Models._getDefaultCtorFor(branchAttribute.NewBaseModelType);
          if (archetype.Id.Key.Contains("Character")) {
            System.Diagnostics.Debugger.Break();
          }
          (archetype as IFactory).ModelConstructor
            = defaultModelConstructor;
        }


        // Try to make the default model, and register what that is:
        defaultModel
          = Configuration.Loader.GetOrBuildTestModel(
              archetype,
              archetype.ModelTypeProduced
          );
      }
      catch (CannotInitializeTypeException e) {
        exception = e;
      }
      catch (MissingDependencyForModelException e) {
        exception = new FailedToConfigureNewModelException($"Could not configure default model. Will try again.", e);
      }
      catch (FailedToConfigureNewModelException e) {
        exception = new FailedToConfigureNewArchetypeException($"Could not configure default model. Will try again.", e);
      }
      catch (Exception e) {
        exception = new FailedToConfigureNewModelException($"Could not configure default model. Will try again.", e);
      }

      if (exception is not null) {
        return false;
      }

      System.Type modelType = defaultModel.GetType();

      archetype.ModelTypeProduced
        = Universe.Models._modelTypesProducedByArchetypes[archetype]
        = modelType;

      if (!Universe.Archetypes._rootArchetypeTypesByBaseModelType.ContainsKey(modelType.FullName)) {
        Universe.Archetypes._rootArchetypeTypesByBaseModelType[modelType.FullName] = archetype.GetType();
      }

      Universe._extraContexts
        .ForEach(context => context.Value.OnTestModelBuilt(archetype,  modelType, defaultModel ));

      return true;
    }

    #endregion

    #region Model Init

    /// <summary>
    /// Try to initialize a model type.
    /// </summary>
    bool _tryToPreInitializeSimpleModel(Type systemType, out Exception e) {
      /// Check dependencies
      if (Universe.Models.Dependencies.TryGetValue(systemType, out var dependencies)) {
        Type firstUnloadedDependency
          = dependencies.FirstOrDefault(t => !_initializedTypes.Contains(t));
        if (firstUnloadedDependency != null) {
          e = new MissingDependencyForModelException(systemType, firstUnloadedDependency);
          return false;
        }
      }

      // invoke static ctor

      try {
        _runStaticCtorsFromBaseClassUp(systemType);
      }
      catch (CannotInitializeModelException ce) {
        if (Options.FatalOnCannotInitializeType) {
          throw ce;
        }

        e = ce;
        return false;
      }
      catch (Exception ex) {
        e = new FailedToConfigureNewModelException($"Could not initialize Model of type {systemType} due to Unknown Inner Exception.", ex);
        return false;
      }

      e = null;
      return true;
    }

    /// <summary>
    /// Try to initialize a model type.
    /// </summary>
    bool _tryToInitializeModel(Type systemType, out Exception e) {
      /// Check dependencies
      if (Universe.Models.Dependencies.TryGetValue(systemType, out var dependencies)) {
        Type firstUnloadedDependency
          = dependencies.FirstOrDefault(t => !_initializedTypes.Contains(t));
        if (firstUnloadedDependency != null) {
          e = new MissingDependencyForModelException(systemType, firstUnloadedDependency);
          return false;
        }
      }

      // invoke static ctor

      try {
        _runStaticCtorsFromBaseClassUp(systemType);
        _registerModelType(systemType);
      } catch (CannotInitializeModelException ce) {
        if (Options.FatalOnCannotInitializeType) {
          throw ce;
        }

        e = ce;
        return false;
      } catch (Exception ex) {
        e = new FailedToConfigureNewModelException($"Could not initialize Model of type {systemType} due to Unknown Inner Exception.", ex);
        return false;
      }

      _initializedTypes.Add(systemType);
      Universe._extraContexts
        .ForEach(context => context.Value.OnModelTypeWasInitialized(systemType));

      e = null;
      return true;
    }

    /// <summary>sd
    /// Register a new type of model.
    /// </summary>
    void _registerModelType(Type systemType) {

      systemType.GetMethod(
        nameof(IModel.Setup),
        BindingFlags.Instance
          | BindingFlags.NonPublic
          | BindingFlags.Static
      )?.Invoke(null, new object[] { Universe });

      //bool skipTestBuildingModel = false;

      // assign root archetype references
      if (!Universe.Archetypes._rootArchetypeTypesByBaseModelType.ContainsKey(key: systemType.FullName) 
        && systemType.IsAssignableToGeneric(typeof(IModel<,>))
      ) {
        var types = systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<,>));
        if (!typeof(IModel.IBuilderFactory).IsAssignableFrom(types.Last())) {
          Type rootArchetype = types.Last();

          Universe.Archetypes._rootArchetypeTypesByBaseModelType[systemType.FullName]
            = rootArchetype;

          // if the type is an unasigned generic:
          /*if (rootArchetype.FullName == null) {
            skipTestBuildingModel = true;
          }*/
        }
      }

      _initializedArchetypes.Add(
        _getDefaultFactoryBuilderForModel(systemType)
      );

      try {
        Universe.Models._baseTypes.Add(
          systemType.FullName,
          systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<>)).FirstOrDefault()
            ?? systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<,>)).First()
        );
      }
      catch (Exception e) {
        throw new NotImplementedException($"Could not find IModel<> Base Type for {systemType}, does it inherit from IModel by mistake instead of Model<T>?", e);
      }

      Universe._extraContexts
        .ForEach(context => context.Value.OnModelTypeWasRegistered(systemType));
    }

    #endregion

    #region Component Init

    /// <summary>
    /// Try to initialize a component type.
    /// </summary>
    bool _tryToInitializeComponent(Type systemType, out Exception e) {
      /// Check dependencies
      if (Universe.Components.Dependencies.TryGetValue(systemType, out var dependencies)) {
        Type firstUnloadedDependency
          = dependencies.FirstOrDefault(t => !_initializedTypes.Contains(t));
        if (firstUnloadedDependency != null) {
          e = new MissingDependencyForComponentException(systemType, firstUnloadedDependency);
          return false;
        }
      }

      try {
        _registerComponentType(systemType);
      } catch (CannotInitializeComponentException ce) {
        if (Options.FatalOnCannotInitializeType) {
          throw ce;
        }

        e = ce;
        return false;
      } catch (Exception ex) {
        e = new FailedToConfigureNewComponentException($"Could not initialize Component of type {systemType} due to Unknown Inner Exception.", ex);
        return false;
      }

      _initializedTypes.Add(systemType);
      e = null;
      return true;
    }

    /// <summary>
    /// Register types of components
    /// </summary>
    void _registerComponentType(Type systemType) {

      // invoke static ctor
      _runStaticCtorsFromBaseClassUp(systemType);

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
          systemType.GetFirstInheritedGenericTypeParameters(typeof(IComponent<>)).First()
        );
      } catch (Exception e) {
        throw new NotImplementedException($"Could not find IComponent<> Base Type for {systemType}, does it inherit from IComponent instead of IComponent<T> by mistake?", e);
      }

      if (typeof(IComponent.IHaveContract).IsAssignableFrom(systemType)) {
        _cacheContractsForComponentType(systemType);
      }
    }

    void _cacheContractsForComponentType(Type systemType) {
      foreach (var contractTypeSets in systemType.GetAllInheritedGenericTypeParameters(typeof(IComponent<>.IHaveContractWith<>))) {
        (Type a, Type b, _) = contractTypeSets.ToList();
        MethodInfo contract = systemType.GetInterfaceMap(typeof(IComponent<>.IHaveContractWith<>).MakeGenericType(a, b)).TargetMethods.First(m => {
          if (m.Name.EndsWith("ExecuteContractWith"))
            if (m.GetParameters().Length == 1)
              if (m.GetParameters().First().ParameterType == b)
                if (m.ReturnType == typeof(ValueTuple<,>).MakeGenericType(a, b))
                  return true;
          return false;
        });

        if (IComponent.IHaveContract._contracts.TryGetValue(a, out var existingDic)) {
          existingDic.Add(b, _buildContractExecutor(contract));
        }
        else IComponent.IHaveContract._contracts[a] = new() {
          {b, _buildContractExecutor(contract)}
        };
      }
    }

    static Func<IComponent, IComponent, (IComponent a, IComponent b)> _buildContractExecutor(MethodInfo contract) => (a, b) => {
      var @return = (ITuple)contract.Invoke(a, new[] { b });
      return ((Data.IComponent)@return[0], (Data.IComponent)@return[1]);
    };

    HashSet<System.Type> _staticallyInitializedTypes
      = new();

    #endregion

    #region Enum Init

    void _registerEnumValue(PropertyInfo prop) {
      try {
        prop.GetValue(null);
      }
      catch (Exception e) {
        _failedEnumerations.Add(prop, e);
      }
    }

    #endregion

    #region Modifications Init

    /// <summary>
    /// Call all the the Archetype.Modifier.Initialize() functions in mod load order.
    /// </summary>
    void _applyModificationsToAllTypesByAssemblyLoadOrder() {
      foreach (System.Type modifierType in _assemblyTypesToBuild.Values
        .Select(a => a.Modifications)
        .Where(v => !(v is null))
      ) {
        Modifications modifier
          = Activator.CreateInstance(
            modifierType,
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { Universe },
            null
          ) as Modifications;

        modifier.Initialize();
      }
    }

    #endregion

    #endregion

    #region Test Build Models

    /// <summary>
    /// A function that can be used to test build models.
    /// </summary>
    public static IModel GetOrBuildTestModel(System.Type modelBase, Universe universe) {
      if (universe.Loader._staticallyInitializedTypes.Contains(modelBase)) {
        var factory = universe.Archetypes.GetDefaultForModelOfType(modelBase);
        return GetOrBuildTestModel(factory, modelBase);
      } else throw new FailedToConfigureNewModelException($"Cannot make a default test model with an unknown archetype if it's static constructors have not yet been called as a different archetype may have been set inthe static ctor.");
    }

    /// <summary>
    /// A function that can be used to test build models.
    /// </summary>
    public static IModel GetOrBuildTestModel(Archetype factory, System.Type modelBase) {
      // check the cache (if there is one still)
      if ((factory.Id.Universe.Loader._testModels?.ContainsKey(factory.GetType()) ?? false)
        && factory.Id.Universe.Loader._testModels[factory.GetType()].ContainsKey(modelBase)
      ) {
        return factory.Id.Universe.Loader._testModels[factory.GetType()][modelBase];
      }

      IBuilder testBuilder
        = _loadOrGetTestBuilder(
          factory,
          factory.Id.Universe.Loader._loadOrGetTestParams(factory, modelBase),
          out (IModel model, bool hasValue)? potentiallyBuiltModel
        );

      IModel testModel;
      try {
        testModel = potentiallyBuiltModel?.model ?? factory.MakeDefaultWith(testBuilder);
      }
      catch (Exception e) {
        Type accurateTargetType = _tryToCalculateAcurateBuilderType(modelBase, e);
        testModel = GetOrBuildTestModel(factory, accurateTargetType);
      }

      /// cache if this is the final form of le model.
      if (factory.Id.Universe.Loader._testModels is not null && factory._modelAutoBuilderSteps is not null) {
        System.Type constructedModelType = testModel.GetType();
        if (factory.Id.Universe.Loader._testModels.TryGetValue(factory.GetType(), out var existing)) {
          existing[constructedModelType] = testModel;
        }
        else {
          factory.Id.Universe.Loader._testModels[factory.GetType()] = new() {
              {constructedModelType, testModel }
            };
        }
      }

      return testModel;
    }

    static Type _tryToCalculateAcurateBuilderType(Type modelBase, Exception e) {
      System.Type accurateTargetType;

      // try to get it from the auto builder exception.
      if (e is AutoBuildAttribute.Exception autoBuilderFailure && autoBuilderFailure.ModelTypeBeingBuilt != modelBase) {
        accurateTargetType = autoBuilderFailure.ModelTypeBeingBuilt;
      } // try to re-target via the called constructor.
      else if (e is IModel.Builder.Param.IException) {
        StackTrace stackTrace = new(e);
        StackFrame lastFrame = stackTrace.GetFrame(1);
        Console.WriteLine(lastFrame);
        MethodBase method = lastFrame.GetMethod();
        if (method.Name.Contains("ctor")) {
          accurateTargetType = lastFrame.GetMethod().DeclaringType;
        }
        else {
          StackFrame[] frames = stackTrace.GetFrames();
          lastFrame = frames.First(f => {
            method = f.GetMethod();
            bool nameMatch = method.Name.Contains("ctor") && !method.Name.Contains("ModelConstructor");
            if (!nameMatch) {
              return false;
            }
            bool typeMatch = modelBase.IsAssignableFrom(method.DeclaringType);
            return typeMatch;
          });
          accurateTargetType = lastFrame.GetMethod().DeclaringType;
        }

        /*var testParams = _loadOrGetTestParams(factory, accurateTargetType);
        builder = _loadOrGetTestBuilder(factory, out var potentialModel);
        defaultModel = potentialModel?.model ?? factory.MakeDefaultWith(builder);*/
      }
      else throw e;
      return accurateTargetType;
    }

    /// <summary>
    /// Test build a model of the given type using it's default archetype or builder.
    /// </summary>
    IModel _testBuildDefaultModel(Type systemType) {
      Archetype defaultFactory = _getDefaultFactoryBuilderForModel(systemType);

      if (defaultFactory == null) {
        throw new Exception($"Could not make a default model for model of type: {systemType.FullName}. Could not fine a default BuilderFactory or Archetype to build it with.");
      }

      IModel defaultModel;

      try {
        defaultModel
          = Configuration.Loader.GetOrBuildTestModel(
              defaultFactory,
              systemType
          );

        Universe.Models._modelTypesProducedByArchetypes[defaultFactory] = defaultModel.GetType();
      }
      catch (Exception e) {
        throw new FailedToConfigureNewModelException($"Could not make a default model for model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}.", e);
      }

      return defaultModel;
    }

    Archetype _getDefaultFactoryBuilderForModel(Type systemType) 
      => systemType.IsAssignableToGeneric(typeof(IModel<,>))
        ? Universe.Archetypes.GetDefaultForModelOfType(systemType)
        : Universe.Models.GetBuilderFactoryFor(systemType) as Archetype;

    /// <summary>
    /// Test build a model of the given type using it's default archetype or builder.
    /// </summary>
    void _testBuildDefaultComponent(Type systemType) {
      if (!(Universe.Components.GetBuilderFactoryFor(systemType) is Archetype defaultFactory)) {
        throw new Exception($"Could not make a default component model of type: {systemType.FullName}. Could not fine a default BuilderFactory to build it with.");
      }

      try {
        IModel defaultComponent
          = Configuration.Loader.GetOrBuildTestModel(
             defaultFactory,
             systemType
          );

        /// Register component key
        Universe.Components.
          _byKey[(defaultFactory as IComponent.IBuilderFactory).Key]
            = systemType;
      }
      catch (Exception e) {
        throw new Exception($"Could not make a default component model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}. Make sure you have propper default test parameters set for the archetype", e);
      }
    }

    static IBuilder _loadOrGetTestBuilder(Archetype factory, Dictionary<string, object> @params, out (IModel model, bool hasValue)? potentiallyBuiltModel) {
      IFactory iFactory = factory;
      if (iFactory.ModelConstructor is null) {
        Func<IBuilder, IModel> defaultCtor = factory.Id.Universe.Models
          ._getDefaultCtorFor(factory.ModelTypeProduced);

        if (factory.Id.Key.Contains("Character")) {
          System.Diagnostics.Debugger.Break();
        }
        iFactory.ModelConstructor
          = builder => defaultCtor.Invoke(builder);
      }

      // TODO: is this cache check needed?
      potentiallyBuiltModel = factory.Id.Universe.Loader._testModels is not null
        && factory.Id.Universe.Loader._testModels.ContainsKey(factory.GetType())
        && factory.Id.Universe.Loader._testModels[factory.GetType()].ContainsKey(factory.ModelTypeProduced)
          ? (factory.Id.Universe.Loader._testModels[factory.GetType()][factory.ModelTypeProduced], true)
          : null;

      if (potentiallyBuiltModel is null) {
        Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor
            = factory.GetGenericBuilderConstructor();

        IBuilder builder = builderCtor.Invoke(
          factory,
          @params
        ); ;

        return builder;
      }

      return null;
    }

    Dictionary<string, object> _loadOrGetTestParams(Archetype factoryType, System.Type modelType) {
      System.Type currentModelType = null;
      if (!factoryType.Id.Universe.Loader.IsFinished) {
        if (_loadedTestParams?.TryGetValue(factoryType, out currentModelType) ?? false) {
          if (currentModelType != modelType) {
            return _generateTestParamsHelper(factoryType, modelType);
          }
        } else {
          return _generateTestParamsHelper(factoryType, modelType);
        }
      }


      return factoryType.DefaultTestParams;
    }

    Dictionary<string, object> _generateTestParamsHelper(Archetype factoryType, Type modelType) {
      Dictionary<string, object> @params = TestValueAttribute._generateTestParameters(factoryType, modelType);

      _loadedTestParams?.Set(factoryType, modelType);

      return @params.Any()
        ? (factoryType._defaultTestParams = @params)
        : (factoryType._defaultTestParams = null);
    }

    #endregion

    #region Try To Complete Init Loops

    /// <summary>
    /// Try to initialize any archetypes that failed:
    /// </summary>
    void _tryToCompleteAllComponentsInitialization() {
      _uninitializedComponents.Keys.ToList().ForEach(componentSystemType => {
        if (_tryToInitializeComponent(componentSystemType, out var e)) {
          _uninitializedComponents.Remove(componentSystemType);
        } else {
          _uninitializedComponents[componentSystemType] = e;
        }
      });
    }

    /// <summary>
    /// Try to initialize any archetypes that failed:
    /// </summary>
    void _tryToCompleteAllModelsInitialization() {
      _uninitializedModels.Keys.ToList().ForEach(modelSystemType => {
        if (_tryToInitializeModel(modelSystemType, out var e)) {
          _uninitializedModels.Remove(modelSystemType);
        } else {
          _uninitializedModels[modelSystemType] = e;
        }
      });
    }

    /// <summary>
    /// Try to initialize any archetypes that failed:
    /// </summary>
    void _tryToCompleteAllArchetypesInitialization() {
      _uninitializedArchetypes.Keys.ToList().ForEach(archetypeSystemType => {
        if (_tryToInitializeArchetype(archetypeSystemType, out var e)) {
          _uninitializedArchetypes.Remove(archetypeSystemType);
        } else {
          _uninitializedArchetypes[archetypeSystemType] = e;
        }
      });
    }

    #endregion

    #region Finishing and Finalization

    /// <summary>
    /// Try to finish all remaining initialized archetypes:
    /// </summary>
    void _tryToFinishAllInitalizedTypes() {
      var values = _initializedArchetypes.ToList();
      values.RemoveAll(archetype => {
        try {
          archetype.Finish();

          return true;
        } // attempt failure: 
        catch(FailedToConfigureNewArchetypeException) {

          return false;
        } // attempt fatal: 
        catch(CannotInitializeArchetypeException) {
          if(Options.FatalOnCannotInitializeType) {
            throw;
          }

          return true;
        } // attempt fatal: 
        catch (Exception) {
          if(Options.FatalOnCannotInitializeType) {
            throw;
          }

          return true;
        }
      });
      _initializedArchetypes = values.ToHashSet();
    }

    /// <summary>
    /// Finish initialization
    /// </summary>
    void _finalize() {
      _reportOnFailedTypeInitializations();
      _finalizeModelSerializerSettings();
      Universe._extraContexts.ToList()
        .ForEach(context => context.Value.OnLoaderFinalize());

      _clearUnnesisaryFields();

      IsFinished = true;
    }

    void _clearUnnesisaryFields() {
      _orderedAssemblyFiles = null;
      _testModels = null;
      _loadedTestParams = null;
      _unorderedAssembliesToLoad = null;
      _assemblyTypesToBuild = null;

      _uninitializedArchetypes = null;
      _uninitializedComponents = null;
      _uninitializedModels = null;

      _failedComponents = null;
      _failedModels = null;
      _failedArchetypes = null;
      _failedEnumerations = null;

      _initializedArchetypes = null;
    }

    void _finalizeModelSerializerSettings() {
      // add the converters to the default json serializer settings.
      bool defaultExists;
      JsonSerializerSettings @default;
      if(JsonConvert.DefaultSettings is not null) {
        @default = JsonConvert.DefaultSettings();
        defaultExists = true;
      }
      else {
        @default = new JsonSerializerSettings();
        defaultExists = false;
      }

      JsonConvert.DefaultSettings = () => {
        return @default.UpdateJsonSerializationSettings(Universe, !defaultExists);
      };
    }

    void _reportOnFailedTypeInitializations() {
      List<Failure> failures = new();
      foreach((System.Type componentType, Exception ex) in _uninitializedComponents.Merge(_failedComponents)) {
        Console.Error.WriteLine($"Could not initialize Component Type: {componentType}, due to Internal Exception:\n\n{ex}");
        failures.Add(new("Component", componentType, ex));
      }
      foreach((System.Type modelType, Exception ex) in _uninitializedModels.Merge(_failedModels)) {
        Console.Error.WriteLine($"Could not initialize Model Type: {modelType}, due to Internal Exception:\n\n{ex}");
        failures.Add(new("Model", modelType, ex));
      }
      foreach((System.Type archetypeType, Exception ex) in _uninitializedArchetypes.Merge(_failedArchetypes)) {
        Console.Error.WriteLine($"Could not initialize Archetype Type: {archetypeType}, due to Internal Exception:\n\n{ex}");
        failures.Add(new("Archetype", archetypeType, ex));
      }
      foreach ((MemberInfo enumProp, Exception ex) in _failedEnumerations) {
        Console.Error.WriteLine($"Could not initialize Enum of Type: {(enumProp is PropertyInfo p ? p.PropertyType : "unknown")} on property with name:{enumProp.Name} on type: {enumProp.DeclaringType.FullName} due to Internal Exception:\n\n{ex}");
        failures.Add(new("Enumeration", (enumProp as PropertyInfo).PropertyType, ex));
      }

      Failures = failures;
      if((Options.FatalOnCannotInitializeType || Options.FatalDuringFinalizationOnCouldNotInitializeTypes) && Failures.Any()) {
        throw new InvalidOperationException("Failed to initialize several types in the ECSBAM Loader:\n"
          + string.Join('\n', Failures));
      }
    }

    /// <summary>
    /// Represents a failed type that wasn't loaded during XBAM initialization
    /// </summary>
    public struct Failure {

      /// <summary>
      /// The XBam Type of the failure.
      /// Buit in options are: Archetype, Model, Enumeration and Component.
      /// </summary>
      public string XbamType { get; }

      /// <summary>
      /// The system type of the failed type
      /// </summary>
      public Type SystemType { get; }

      /// <summary>
      /// The exception that was thrown.
      /// </summary>
      public Exception Exception { get; }

      /// <summary>
      /// Make a new failure for reporting.
      /// </summary>
      public Failure(string xbamType, Type systemType, Exception exception) {
        XbamType = xbamType;
        SystemType = systemType;
        Exception = exception;
      }

      ///<summary><inheritdoc/></summary>
      public override string ToString() {
        StringBuilder builder = new();
        builder.Append($"\n====:{XbamType}::{SystemType.ToFullHumanReadableNameString()}:====");
        builder.Append($"\n\t==Exception:==");
        builder.Append(Exception.Message?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\n\t\t\t\t"));
        builder.Append($"\n\t\t====Stack Trace:==");
        builder.Append(Exception.StackTrace?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\n\t\t\t\t"));
        builder.Append($"\n\t\t======");

        var ie = Exception.InnerException;
        while (ie is not null) {
          builder.Append($"\n\n\t\t====Inner Exception:==");
          builder.Append(ie.Message?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\n\t\t\t\t"));
          builder.Append($"\n\t\t\t======Stack Trace:==");
          builder.Append(ie.StackTrace?.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\n\t\t\t\t"));
          builder.Append($"\n\t\t\t========");
          builder.Append($"\n\t\t======");
          ie = ie.InnerException;
        }

        builder.Append($"\n\t====");
        builder.Append($"========");

        return builder.ToString();
      }
    }

    #endregion

    /// <summary>
    /// Go up the tree and find a declaring type that these types inherit from.
    /// </summary>
    static Type _getFirstDeclaringParent(Type archetypeSystemType) {
      if (archetypeSystemType.DeclaringType == null) {
        if (archetypeSystemType.BaseType != null) {
          return _getFirstDeclaringParent(archetypeSystemType.BaseType);
        }
        else
          return null;
      }
      else
        return archetypeSystemType.DeclaringType;
    }
  }
}