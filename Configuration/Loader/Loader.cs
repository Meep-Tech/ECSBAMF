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
    internal List<Assembly> _assemblyLoadOrder
        = new List<Assembly>();

    /// <summary>
    /// Types that failed to initialize and their exceptions.
    /// </summary>
    public IEnumerable<System.Type> InitializedTypes
      => _initializedTypes; List<System.Type> _initializedTypes
        = new();

    /// <summary>
    /// The assembly types that will be built in order
    /// </summary>
    OrderedDictionary<Assembly, AssemblyBuildableTypesCollection> _assemblyTypesToBuild;

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedArchetypes
        = new();

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<System.Type, Exception> _failedArchetypes
        = new();

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<MemberInfo, Exception> _failedEnumerations
        = new();

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<System.Type, Exception> _failedModels
        = new();

    /// <summary>
    /// The types that failed entirely
    /// </summary>
    Dictionary<System.Type, Exception> _failedComponents
        = new();

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedModels
        = new();

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    Dictionary<System.Type, Exception> _uninitializedComponents
        = new();

    /// <summary>
    /// The types we need to construct and map data to
    /// </summary>
    List<Archetype> _initializedArchetypes
        = new();

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
    Map<ushort, string> _orderedAssemblyFiles
      = new();

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

      Universe._extraContexts.ToList().ForEach(context => context.Value.OnAllTypesInitializationComplete());

      _applyModificationsToAllTypesByAssemblyLoadOrder();

      Universe._extraContexts.ToList().ForEach(context => context.Value.OnModificationsComplete());

      while (_remainingFinalizationAttempts-- > 0 && _initializedArchetypes.Count > 0) {
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
      Universe.ModelSerializer
        = new Model.Serializer(Options.ModelSerializerOptions, Universe);
    }

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

    /// <summary>
    /// Collect all assemblies that could have archetypes into _unorderedAssembliesToLoad
    /// </summary>
    void _loadValidAssemblies() {
      // load internal assemblies
      List<Assembly> defaultAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
      if (Options.PreLoadAllReferencedAssemblies) {
        var moreAssemblies = defaultAssemblies.SelectMany(
          a => a.GetReferencedAssemblies()
            .Where(assembly => assembly.FullName.StartsWith(Options.ArchetypeAssembliesPrefix)
              && !Options.AssemblyPrefixesToIgnore
                .Where(assemblyPrefix => assembly.FullName.StartsWith(assemblyPrefix))
                .Any())
            .Select((item) => {
              try {
                return Assembly.Load(item);
              } catch (Exception e) {
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
          || (assembly.GetName().FullName.StartsWith(Options.ArchetypeAssembliesPrefix)
            && !Options.AssemblyPrefixesToIgnore
              .Where(assemblyPrefix => assembly.GetName().FullName.StartsWith(assemblyPrefix))
              .Any()))
      ).ToHashSet().ToList();
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
      if(_orderedAssemblyFiles.Forward.Any()) {
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

    void _initializeTypesByAssembly() {
      foreach(AssemblyBuildableTypesCollection typesToBuild in _assemblyTypesToBuild.Values) {
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

        // then initialize archetypes:
        foreach(Type systemType in typesToBuild.Archetypes) {
          if(!_tryToInitializeArchetype(systemType, out var e)) {
            if (e is CannotInitializeTypeException) {
              _failedArchetypes[systemType] = e;
            } else 
              _uninitializedArchetypes[systemType] = e;
          }
        }

        // then register models
        foreach(Type systemType in typesToBuild.Models) {
          if (!_tryToInitializeModel(systemType, out var e)) {
            if (e is CannotInitializeTypeException) {
              _failedModels[systemType] = e;
            } else
              _uninitializedModels[systemType] = e;
          }
        }
      }
    }

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

      bool isSplayed = typeof(ISplayed).IsAssignableFrom(systemType);

      Archetype archetype = null;
      try {
        if (!isSplayed || (!Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildInInitialLoadAttribute))
              && !Attribute.IsDefined(systemType, typeof(Settings.DoNotBuildThisOrChildrenInInitialLoadAttribute), true))) {
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

      if (isSplayed) {
        object dummy = FormatterServices.GetUninitializedObject(systemType);
        foreach (var splayType in systemType.GetAllInheritedGenericTypes(typeof(Archetype.IBuildOneForEach<,>))) {
          if(!typeof(ISplayedLazily).IsAssignableFrom(splayType)) {
            MethodInfo getMethod 
              = splayType.GetMethod("ConstructArchetypeFor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach(var @enum in Universe.Enumerations.GetAllByType(splayType.GetGenericArguments().First())) {
              var newType = getMethod.Invoke(dummy, new[] { @enum });
              Universe._extraContexts
                .ForEach(context => context.Value.OnArchetypeWasInitialized(systemType, (Archetype)newType));
            }
          }
        }
      }

      _initializedTypes.Add(systemType);
      if (archetype is not null) {
        Universe._extraContexts
          .ForEach(context => context.Value.OnArchetypeWasInitialized(systemType, archetype));
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

      try {
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

    /// <summary>sd
    /// Register a new type of model.
    /// </summary>
    void _registerModelType(Type systemType) {

      // invoke static ctor
      _runStaticCtorsFromBaseClassUp(systemType);

      systemType.GetMethod(
        nameof(IModel.Setup),
        BindingFlags.Instance
          | BindingFlags.NonPublic
          | BindingFlags.Static
      )?.Invoke(null, new object[] { Universe });

      // assign root archetype references
      if(systemType.IsAssignableToGeneric(typeof(IModel<,>))) {
        var types = systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<,>));
        if(!typeof(IModel.IBuilderFactory).IsAssignableFrom(types.Last())) {
          Universe.Archetypes._rootArchetypeTypesByBaseModelType[systemType.FullName] 
            = types.Last();
        }
      }

      var defaultModel = _testBuildDefaultModel(systemType);
      try {
        Universe.Models._baseTypes.Add(
          systemType.FullName,
          systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<>)).FirstOrDefault()
            ?? systemType.GetFirstInheritedGenericTypeParameters(typeof(IModel<,>)).First()
        );
      } catch(Exception e) {
        throw new NotImplementedException($"Could not find IModel<> Base Type for {systemType}, does it inherit from IModel by mistake instead of Model<T>?", e);
      }

      Universe._extraContexts
        .ForEach(context => context.Value.OnModelTypeWasRegistered(systemType, defaultModel));
    }

    void _registerEnumValue(PropertyInfo prop) {
      try {
        prop.GetValue(null);
      } catch (Exception e) {
        _failedEnumerations.Add(prop, e);
      }
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

      if(typeof(IComponent.IHaveContract).IsAssignableFrom(systemType)) {
        _cacheContractsForComponentType(systemType);
      }
    }

    void _cacheContractsForComponentType(Type systemType) { 
      foreach(var contractTypeSets in systemType.GetAllInheritedGenericTypeParameters(typeof(IComponent<>.IHaveContractWith<>))) {
        (Type a, Type b, _) = contractTypeSets.ToList();
        MethodInfo contract = systemType.GetInterfaceMap(typeof(IComponent<>.IHaveContractWith<>).MakeGenericType(a, b)).TargetMethods.First(m => {
          if(m.Name.EndsWith("ExecuteContractWith"))
            if(m.GetParameters().Length == 1)
              if(m.GetParameters().First().ParameterType == b)
                if(m.ReturnType == typeof(ValueTuple<,>).MakeGenericType(a, b))
                  return true;
          return false;
        });

        if(IComponent.IHaveContract._contracts.TryGetValue(a, out var existingDic)) {
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
      = new ();

    void _runStaticCtorsFromBaseClassUp(System.Type @class) {
      List<System.Type> newAncestors = new() {
        @class
      };
      while((@class = @class.BaseType) != null) {
        if(_staticallyInitializedTypes.Contains(@class)) {
          break;
        } else {
          _staticallyInitializedTypes.Add(@class);
          newAncestors.Add(@class);
        }
      }

      newAncestors.Reverse();
      foreach(System.Type type in newAncestors) {
        try {
          // invoke static ctor
          System.Runtime.CompilerServices
            .RuntimeHelpers
            .RunClassConstructor(type.TypeHandle);
        } catch(Exception e) {
          throw new Exception($"Failed to run static constructor for ancestor: {type?.FullName ?? "null"}, of type: {@class.FullName}.\n=Exception:{e}\n\n=Inner Exception\n{e.InnerException}");
        }
      }
    }

    /// <summary>
    /// Test build a model of the given type using it's default archetype or builder.
    /// </summary>
    IModel _testBuildDefaultModel(Type systemType) {
      Archetype defaultFactory;
      if(systemType.IsAssignableToGeneric(typeof(IModel<,>))) {
        defaultFactory 
          = Universe.Archetypes.GetDefaultForModelOfType(systemType);
      }
      else {
        defaultFactory = Universe.Models.GetBuilderFactoryFor(systemType) as Archetype;
      }

      if(defaultFactory == null) {
        throw new Exception($"Could not make a default model for model of type: {systemType.FullName}. Could not fine a default BuilderFactory or Archetype to build it with.");
      }

      IModel defaultModel;
      try {
        defaultModel
          = Configuration.Loader.TestBuildModel(
              defaultFactory,
              systemType
          );
        /*
        Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor
          = defaultFactory.GetGenericBuilderConstructor();
        IBuilder builder = builderCtor.Invoke(
          defaultFactory,
          _loadTestParams(defaultFactory, systemType)
        );
        defaultModel = defaultFactory.MakeDefaultWith(builder);*/
        Universe.Models._modelTypesProducedByArchetypes[defaultFactory] = defaultModel.GetType();
      }
      catch (Exception e) {
        throw new Exception($"Could not make a default model for model of type: {systemType.FullName}, using default archeytpe of type: {defaultFactory}.", e);
      }

      return defaultModel;
    }

    /// <summary>
    /// Test build a model of the given type using it's default archetype or builder.
    /// </summary>
    void _testBuildDefaultComponent(Type systemType) {
      if(!(Universe.Components.GetBuilderFactoryFor(systemType) is Archetype defaultFactory)) {
        throw new Exception($"Could not make a default component model of type: {systemType.FullName}. Could not fine a default BuilderFactory to build it with.");
      }

      try {/*
        Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor 
          = defaultFactory.GetGenericBuilderConstructor();
        IBuilder builder = builderCtor.Invoke(
          defaultFactory,
          _loadTestParams(defaultFactory, systemType)
        );
        defaultFactory.MakeDefaultWith(builder);*/

        IModel defaultComponent
          = Configuration.Loader.TestBuildModel(
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

    /// <summary>
    /// A function that can be used to test build models.
    /// </summary>
    public static IModel TestBuildModel(Archetype factory, System.Type modelBase) {
			IModel defaultModel;
			Dictionary<string, object> testParams 
        = _loadTestParams(factory, modelBase);
			IBuilder builder 
        = _makeTestBuilder(factory, testParams);

			try {
				defaultModel = factory.MakeDefaultWith(builder);
			}
			catch (Exception e) {
        // try to re-target via the called constructor.
        if (e is IModel.Builder.Param.IException) {
					StackTrace stackTrace = new(e);
					StackFrame lastFrame = stackTrace.GetFrame(1);
          Console.WriteLine(lastFrame);
					MethodBase method = lastFrame.GetMethod();
          System.Type accurateTargetType;
          if (method.Name == "ctor") {
            accurateTargetType = lastFrame.GetMethod().DeclaringType;
          }
          else {
            lastFrame = stackTrace.GetFrames().First(f => {
				      method = f.GetMethod();
              bool nameMatch = method.Name == "ctor";
              bool typeMatch = modelBase.IsAssignableFrom(method.DeclaringType);
              return nameMatch && typeMatch;
            });
            accurateTargetType = lastFrame.GetMethod().DeclaringType;
          }

          testParams = _loadTestParams(factory, accurateTargetType);
          builder = _makeTestBuilder(factory, testParams);
          defaultModel = factory.MakeDefaultWith(builder);
        }
        else throw e;
			}

			return defaultModel;
		}

		static IBuilder _makeTestBuilder(Archetype factory, Dictionary<string, object> testParams) {
			Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor
					= factory.GetGenericBuilderConstructor();
			IBuilder builder = builderCtor.Invoke(
			  factory,
			  testParams
			);
			return builder;
		}

		// TODO: clear from memory when loading is done.
		static HashSet<Archetype> _loadedTestParams
      = new();

    internal static Dictionary<string, object> _loadTestParams(Archetype factoryType, System.Type modelType) {
      if (!_loadedTestParams.Contains(factoryType)) {
        Dictionary<string, object> @params = factoryType.DefaultTestParams ?? new();
        foreach ((PropertyInfo property, TestValueAttribute attribute) in modelType
          .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
          .Select(p => (p, a: p.GetCustomAttribute<TestValueAttribute>(true)))
          .Where(e => e.a is not null)
        ) {
          if (attribute is TestValueIsNewAttribute) {
            attribute._value ??= Activator.CreateInstance(property.PropertyType);
          }
          else if (attribute is TestValueIsEmptyEnumerableAttribute) {
            attribute._value ??= typeof(Enumerable).GetMethod(nameof(Enumerable.Empty), BindingFlags.Static | BindingFlags.Public)
              .MakeGenericMethod(property.PropertyType.GetGenericArguments().First()).Invoke(null, new object[0] );
          } else if (attribute is GetTestValueFromMemberAttribute memberAttribute && memberAttribute.Value is null) {
            try {
              System.Type currentModelType = modelType;
              MemberInfo[] members = new MemberInfo[0];
              MemberInfo member;
              while (!members.Any() && typeof(IModel).IsAssignableFrom(currentModelType)) {
                members = currentModelType.GetMember(memberAttribute.MethodName, BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static);
                currentModelType = currentModelType.BaseType;
              }

              member = members.First();
              if (member is MethodInfo method) {
                var methodParams = method.GetParameters();
                if (methodParams.Any()) {
                  if (methodParams.Length > 1 || !typeof(Archetype).IsAssignableFrom(methodParams.First().ParameterType)) {
                    throw new ArgumentException($"GetTestValueFromMemberAttribute requires a static property, field, or method (with 0 or 1 parameter(s)). If 1 parameter is provided for a method it must be of type Archetype.");
                  }
                  attribute._value = method.Invoke(null, new object[] {
                    factoryType
                  });
                } else {
                  attribute._value = method.Invoke(null, new object[0]);
                }

              } else if (member is PropertyInfo prop) {
                attribute._value = prop.GetValue(null);
              } else if (member is FieldInfo field) {
                attribute._value = field.GetValue(null);
              }
            } catch (Exception e) {
              throw new MissingMemberException($"Member {memberAttribute.MethodName} not found, or {memberAttribute.MethodName} is not a static property, field, or method (with 0 or 1 parameter(s)). If 1 parameter is provided for a method it must be of type Archetype.", e);
            }
          }

          AutoBuildAttribute autoBuildData;
          string fieldName = property.Name;
          if ((autoBuildData = property.GetCustomAttribute<AutoBuildAttribute>(true)) != null) {
            fieldName = autoBuildData.ParameterName ?? fieldName;
          }

          @params[fieldName] = attribute.Value;
        }

        _loadedTestParams.Add(factoryType);

        return @params.Any() 
          ? (factoryType._defaultTestParams = @params) 
          : (factoryType._defaultTestParams = null);
      }

      return factoryType.DefaultTestParams ?? null;
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
      foreach(Assembly assembly in AssemblyLoadOrder) {
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
            || systemType.IsAssignableToGeneric(typeof(Archetype<,>))
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
      if(!allCollections.ContainsKey(assembly)) {
        allCollections.Add(assembly, new AssemblyBuildableTypesCollection(assembly));
      }
    }

    /// <summary>
    /// Try to construct the archetype, which will register it with it's collections:
    /// TODO: change this so if we are missing a dependency archetype, then this tries to load that one by name, and then adds +1 to a depth parameter (default 0) on this function.
    /// Maybe this could be done more smoothly by pre-emptively registering all ids?
    /// </summary>
    Archetype _constructArchetypeFromSystemType(System.Type archetypeSystemType, int depth = 0) {
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


        // load branch attribute and set the new model ctor if there is one
        BranchAttribute branchAttribute;
        // (first one is newest inherited)
        if((branchAttribute = archetypeSystemType.GetCustomAttributes<BranchAttribute>().FirstOrDefault()) != null) {
          branchAttribute.NewBaseModelType
            // Defaults to decalring type (surrounding type) if one wasn't specified.
            ??= GetFirstDeclaringParent(archetypeSystemType);

          // set it to the produced type for now
          archetype.ModelTypeProduced = branchAttribute.NewBaseModelType;

          Func<IBuilder, IModel> defaultModelConstructor
            = Universe.Models._getDefaultCtorFor(branchAttribute.NewBaseModelType);
          (archetype as IFactory).ModelConstructor 
            = defaultModelConstructor;
        }

        // Try to make the default model, and register what that is:
        /*Func<Archetype, Dictionary<string, object>, IBuilder> builderCtor
          = archetype.GetGenericBuilderConstructor();

        Dictionary<string, object> testParams;
        try {
          testParams = _loadTestParams(archetype, archetype.ModelTypeProduced);
        } catch (Exception e) {
          throw e;
				}

        IBuilder builder = builderCtor.Invoke(
          archetype,
          testParams
        );

        IModel defaultModel = archetype.MakeDefaultWith(builder);*/
        IModel defaultModel
          = Configuration.Loader.TestBuildModel(
              archetype,
              archetype.ModelTypeProduced
          );

        if (!Universe.Archetypes._rootArchetypeTypesByBaseModelType.ContainsKey(defaultModel.GetType().FullName)) {
          Universe.Archetypes._rootArchetypeTypesByBaseModelType[defaultModel.GetType().FullName] = archetype.GetType();
        }

        archetype.ModelTypeProduced
          = Universe.Models._modelTypesProducedByArchetypes[archetype] 
          = defaultModel.GetType();

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

      return archetype;
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

    /// <summary>
    /// Call all the the Archetype.Modifier.Initialize() functions in mod load order.
    /// </summary>
    void _applyModificationsToAllTypesByAssemblyLoadOrder() {
      foreach(System.Type modifierType in _assemblyTypesToBuild.Values
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
      Universe._extraContexts.ToList()
        .ForEach(context => context.Value.OnLoaderFinalize());

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
  }
}