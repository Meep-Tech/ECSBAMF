using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using Meep.Tech.Data.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Meep.Tech.Data.Archetype;

namespace Meep.Tech.Data.Configuration {

  /// <summary>
  /// Used to set up debugging and progress loading bars for xbam.
  /// </summary>
  public class ConsoleProgressLogger : Universe.ExtraContext {
    int? _overallStepsRemaining;
    int? _subProcessStepsRemaining;
    int? _totalCurrentSteps;
    int? _totalCurrentSubProcessSteps;
    Loader _loader;

    const string DebuggerOutputPrefix = "XBAM:";

    const int NumberOfGenericLoaderMajorSteps
      = 2;

    const int LoaderModelTestingStepCountSize
      = 3;

    const int LoaderMajorStepCountSize
      = 1;

    const int LoaderModifierStepCountSize
      = 1;

    const int LoaderAssemblyStepCountSize
      = 1;

    const int LoaderCollectTypesStepCountSize
      = 1;

    /// <summary>
    /// Tells this to print full errors inline
    /// </summary>
    public bool VerboseModeForErrors { get; }

    /// <summary>
    /// Tells this to print non error messages more verbosely
    /// </summary>
    public bool VerboseModeForNonErrors { get; }

    /// <summary>
    /// The overall completeness of the whole loader program
    /// </summary>
    public float? OverallPercentComplete
      => _totalCurrentSteps.HasValue && _overallStepsRemaining.HasValue
        ? Math.Min(1f, Math.Max(0, (float)((float)((_totalCurrentSteps - _overallStepsRemaining)) / _totalCurrentSteps)))
        : null;

    /// <summary>
    /// The current sub process of the loader
    /// </summary>
    public string CurrentSubProcessName {
      get;
      private set;
    } = "Initialization";

    /// <summary>
    /// The current completeness of the current sub process
    /// </summary>
    public float? CurrentSubProcessPercentComplete
      => _totalCurrentSubProcessSteps.HasValue && _subProcessStepsRemaining.HasValue
        ? Math.Min(1f, Math.Max(0, (float)(((float)(_totalCurrentSubProcessSteps - _subProcessStepsRemaining)) / _totalCurrentSubProcessSteps)))
        : null;

    /// <summary>
    /// Make a special logger for the loader.
    /// </summary>
    public ConsoleProgressLogger(bool verboseModeForErrors = true, bool verboseModeForNonErrors = false) {
      VerboseModeForErrors = verboseModeForErrors;
      VerboseModeForNonErrors = verboseModeForNonErrors;
    }

    void _writeMessage(string message, string prefix = null, bool isError = false, Exception exception = null, string verboseNonErrorText = null) {
      string toWrite = (isError ? "!" : "") + DebuggerOutputPrefix;
      if (prefix != null) {
        toWrite += prefix + ":";
      }
      if (_overallStepsRemaining != null) {
        toWrite += Math.Round(OverallPercentComplete.Value * 100).ToString() + "%:";
      } else {
        toWrite += "\t";
      }

      toWrite += "\t";

      if (_subProcessStepsRemaining != null) {
        toWrite += (CurrentSubProcessName is not null ? CurrentSubProcessName + " - " : "") + Math.Round(CurrentSubProcessPercentComplete.Value * 100).ToString() + "%:";
        toWrite += "\t";
      }

      if (isError && VerboseModeForErrors) {
        toWrite += (" " + (isError ? "ERROR:" : "") + message + (exception is not null ? $"\n ERROR:\n{exception.ToString().Replace("\n\r", "\n").Replace("\n", "\n\t\t")}\n\n" : ""));
      } else {
        toWrite += message;
      }

      if (!string.IsNullOrWhiteSpace(verboseNonErrorText) && VerboseModeForNonErrors) {
        toWrite += ($"\n" + verboseNonErrorText).Replace("\n\r", "\n").Replace("\n", "\n\t\t");
      }

      Console.WriteLine(toWrite);
    }

    protected internal override Action<Loader> OnLoaderInitializationStart
      => loader => _writeMessage($"Initializing a New XBam Loader.", "Loader", verboseNonErrorText: $"With settings: \n{JsonConvert.SerializeObject(_loader.Options, Formatting.Indented)}");

    protected internal override Action<Universe> OnLoaderInitializationComplete
      => universe => {
        _loader = universe.Loader;
        _writeMessage($"Initialized XBam Loader for Universe: {universe.Key}", "Loader");
      };

    protected internal override Action<IEnumerable<Assembly>> OnLoaderAssembliesCollected
      => assemblies => {
        _overallStepsRemaining
          = _totalCurrentSteps
          = (assemblies.Count() * LoaderAssemblyStepCountSize)
            + (NumberOfGenericLoaderMajorSteps * LoaderMajorStepCountSize) + LoaderCollectTypesStepCountSize + LoaderModelTestingStepCountSize;

        _writeMessage($"Collected and Ordered Assemblies to Load Types from.", verboseNonErrorText: $"Count: {assemblies.Count()}\n{string.Join("\n\t - ", assemblies.Select(a => a.FullName))}\n\n", prefix: "Loader");
      };

    protected internal override Action OnLoaderInitialSystemTypesCollected
      => () => {
        _overallStepsRemaining -= LoaderCollectTypesStepCountSize;
        _writeMessage($"Collected Initial Types to Load from the Ordered Assemblies", "Loader");
      };

    protected internal override Action<Loader.AssemblyBuildableTypesCollection> OnLoaderAssemblyLoadStart
      => (assemblyTypes) => {
        CurrentSubProcessName = $"Load Types From Assembly - {assemblyTypes.Assembly.GetName().Name}";
        _subProcessStepsRemaining
          = _totalCurrentSubProcessSteps
          = assemblyTypes.Components.Count * 2
            + assemblyTypes.Enumerations.Count
            + assemblyTypes.Archetypes.Count
            + assemblyTypes.Models.Count * 2
            + ISplayed._splayedInterfaceTypes.Count;

        if (assemblyTypes.Modifications is not null) {
          _totalCurrentSteps += LoaderModifierStepCountSize;
        }

        _writeMessage($"Started Loading Assembly with {_subProcessStepsRemaining} Types.", "Loader");
      };

    protected internal override Action<PropertyInfo> OnLoaderEnumInitializationStart
      => prop => {
        _writeMessage($"Started Loading Enum from Property: {prop.Name}, on class: {prop.DeclaringType.ToFullHumanReadableNameString()}, of type {prop.PropertyType.ToFullHumanReadableNameString()}.", "Loader");
      };

    protected internal override Action<bool, PropertyInfo, Enumeration, Exception> OnLoaderEnumInitializationComplete
      => (success, prop, @enum, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          _writeMessage($"Finished Loading Enum: {@enum}, from Property: {prop.Name}, on class: {prop.DeclaringType.ToFullHumanReadableNameString()}, of type {prop.PropertyType.ToFullHumanReadableNameString()}.", "Loader");
        }
        else {
          _writeMessage($"Failed to Load Enum from Property: {prop.Name}, on class: {prop.DeclaringType.ToFullHumanReadableNameString()}, of type {prop.PropertyType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    protected internal override Action<Type> OnLoaderComponentInitializationStart
      => componentSystemType => {
        _writeMessage($"Started Loading Component from class: {componentSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    protected internal override Action<Type> OnLoaderBuildTestComponentStart
      => componentSystemType => {
        _writeMessage($"Started Test-Building Component from class: {componentSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    protected internal override Action<bool, Type, IComponent, Exception> OnLoaderBuildTestComponentComplete
      => (success, componentSystemType, component, exception) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          _writeMessage($"Successfully Test-Built Component with Key:{component.Key}, from class: {componentSystemType.ToFullHumanReadableNameString()}", "Loader");
        } else {
          _writeMessage($"Failed to Test-Build Component from class: {componentSystemType.ToFullHumanReadableNameString()}", "Loader", true, exception);
        }
      };

    protected internal override Action<bool, Type, Exception> OnLoaderComponentInitializationComplete
      => (success, componentSystemType, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          _writeMessage($"Finished Loading Component from class: {componentSystemType.ToFullHumanReadableNameString()}", "Loader");
        }
        else {
          _writeMessage($"Failed to Load Component from class: {componentSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    protected internal override Action<Type> OnLoaderSimpleModelInitializationStart
      => modelSystemType => {
        _writeMessage($"Started Initializing Simple Model from class: {modelSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    protected internal override Action<bool, Type, Exception> OnLoaderSimpleModelInitializationComplete
      => (success, modelSystemType, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          _writeMessage($"Finished Initializing Simple Model from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader");
        }
        else {
          _writeMessage($"Failed to Initialize Simple Model from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    protected internal override Action<Type, bool> OnLoaderArchetypeInitializationStart
      => (archetypeSystemType, isSplayedSubType) => {
        _writeMessage($"Started Initializing {(isSplayedSubType ? "A Splayed " : "")}Archetype from class: {archetypeSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    protected internal override Action<bool, Type, Archetype, Exception, bool> OnLoaderArchetypeInitializationComplete
      => (success, archetypeSystemType, archetype, error, isSplayedSubType) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          _writeMessage($"Finished Initializing {(isSplayedSubType ? "Splayed " : "")}Archetype: {archetype}, from class: {archetypeSystemType.ToFullHumanReadableNameString()}.", "Loader");
        }
        else {
          _writeMessage($"Failed to Initialize {(isSplayedSubType ? "A Splayed " : "")}Archetype from class: {archetypeSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    protected internal override Action<Type> OnLoaderModelFullInitializationStart
      => modelSystemType => {
        _writeMessage($"Started Initializing Model Fully from class: {modelSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    protected internal override Action<Type> OnLoaderModelFullRegistrationStart
      => modelSystemType => {
        _writeMessage($"Started Registering Model from class: {modelSystemType.ToFullHumanReadableNameString()}.", "Loader");
      };

    protected internal override Action<bool, Type, Exception> OnLoaderModelFullRegistrationComplete
      => (success, modelSystemType, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          _writeMessage($"Finished Registering Model from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader");
        }
        else {
          _writeMessage($"Failed to Registering Model from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };

    protected internal override Action<bool, Type, Exception> OnLoaderModelFullInitializationComplete
      => (success, modelSystemType, error) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          _writeMessage($"Finished Initializing Model Fully from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader");
        }
        else {
          _writeMessage($"Failed to Initialize Model Fully from class: {modelSystemType.ToFullHumanReadableNameString()}", "Loader", true, error);
        }
      };


    protected internal override Action<Loader.AssemblyBuildableTypesCollection> OnLoaderAssemblyLoadComplete
      => (assemblyTypes) => {
        CurrentSubProcessName = null;
        _subProcessStepsRemaining = null;
        _overallStepsRemaining -= LoaderAssemblyStepCountSize;

        _writeMessage($"Finished Loading Assembly!", "Loader");
      };

    protected internal override Action OnLoaderTypesInitializationFirstRunComplete
      => () => _writeMessage($"First Initialization Attempt Run on all Assemblies Complete!", "Loader");

    protected internal override Action<int> OnLoaderFurtherAnizializationAttemptStart
      => runNumber => {
        _subProcessStepsRemaining = 0;
        CurrentSubProcessName = $"Further Initialization Attempt #: {runNumber - 1}";
        _subProcessStepsRemaining += _loader._uninitializedEnums.Any() ? _loader._uninitializedEnums.Count : 0;
        _subProcessStepsRemaining += _loader._uninitializedArchetypes.Any() ? _loader._initializedArchetypes.Count : 0;
        _subProcessStepsRemaining += _loader._uninitializedComponents.Any() ? _loader._uninitializedComponents.Count : 0;
        _subProcessStepsRemaining += _loader._uninitializedModels.Any() ? _loader._uninitializedModels.Count : 0;
        _writeMessage($"Running Initialization Attempt #{runNumber} for {_loader.UninitializedTypesCount} remaining Uninintalized Types.", "Loader");
      };

    protected internal override Action<int> OnLoaderFurtherAnizializationAttemptComplete
      => runNumber => {
        _subProcessStepsRemaining = 0;
        CurrentSubProcessName = null;
        _writeMessage($"Finished Initialization Attempt #{runNumber} with {_loader.UninitializedTypesCount} remaining Uninintalized Types.", "Loader");
      };

    protected internal override Action OnLoaderTypesInitializationAllRunsComplete
      => () => {
        _overallStepsRemaining -= LoaderMajorStepCountSize;
        _writeMessage($"Finished all Loader Initialization Attempts with {_loader.UninitializedTypesCount} remaining Uninintalized Types.", "Loader");
      };

    protected internal override Action<int> OnLoaderBuildAllTestModelsStart
      => archetypesToTestCount => {
        _subProcessStepsRemaining = archetypesToTestCount;
        CurrentSubProcessName = $"Building Test Models";
        _writeMessage($"Building Test Models for {archetypesToTestCount} Initialized Archetypes.", "Loader");
      };

    protected internal override Action<Archetype> OnLoaderTestModelBuildStart
      => archetypeToBuildTestModelFor => _writeMessage($"Building Test Model for Archetype: {archetypeToBuildTestModelFor}.", "Loader");

    protected internal override Action<bool, Archetype, IModel, Exception> OnLoaderTestModelBuildComplete
      => (success, archetypeToTestBuildModelFor, testBuiltModel, exception) => {
        _subProcessStepsRemaining -= 1;

        if (success) {
          _writeMessage($"Finished Building Test Model of type {testBuiltModel.GetType()} for Archetype: {archetypeToTestBuildModelFor}: {testBuiltModel}", "Loader");
        }
        else {
          _writeMessage($"Failed to Build Test Model for Archetype: {archetypeToTestBuildModelFor}.", "Loader", true, exception);
        }
      };

    protected internal override Action OnLoaderBuildAllTestModelsComplete
      => () => {
        _subProcessStepsRemaining = null;
        CurrentSubProcessName = null;
        _overallStepsRemaining -= LoaderModelTestingStepCountSize;
        _writeMessage($"Finished Building Test Models for Initialized Archetypes.", "Loader");
      };

    protected internal override Action<IEnumerable<Type>> OnLoaderAllModificationsStart
      => modifierTypes => {
        _subProcessStepsRemaining = modifierTypes.Count();
        CurrentSubProcessName = $"Loading Modifiers";
        _writeMessage(message: $"Loading and Running Modifier Types in Assembly Load Order.", verboseNonErrorText: $"{string.Join("\n\t - ", modifierTypes.Select(m => m.Name))}\n\n", prefix: "Loader");
      };

    protected internal override Action<Type> OnLoaderModificationStart
      => modType => _writeMessage($"Starting Initialization for Modifier of Type: {modType.Name} from Assembly: {modType.Assembly.GetName().Name}", "Loader");

    protected internal override Action<bool, Type, Modifications, Exception> OnLoaderModificationComplete
      => (success, modType, mod, error) => {
        _subProcessStepsRemaining -= 1;
        _overallStepsRemaining -= LoaderModifierStepCountSize;

        if (success) {
          _writeMessage($"Finished Initializing and Running Modifier of Type: {modType.Name}, from Assembly: {modType.Assembly.GetName().Name}.", "Loader");
        } else {
          _writeMessage($"Failed to Initialize and Run Modifier of Type: {modType.Name}, from Assembly: {modType.Assembly.GetName().Name}.", "Loader", true, error);
        }
      };

    protected internal override Action OnLoaderAllModificationsComplete
      => () => {
        _subProcessStepsRemaining = null;
        CurrentSubProcessName = null;
        _writeMessage($"Finished Initializing and Loading Modifier Classes", "Loader");
      };

    protected internal override Action OnLoaderFinishTypesStart
      => () => _writeMessage($"Starting the Process of Finishing {_loader._initializedArchetypes.Count()} Archetypes", "Loader");

    protected internal override Action OnLoaderFinishTypesComplete
      => () => {
        _overallStepsRemaining -= LoaderMajorStepCountSize;
        _writeMessage($"Finished Loading {_loader._initializedArchetypes.Count()} Archetypes.", "Loader", verboseNonErrorText: string.Join("\n\t - ", _loader._initializedArchetypes));
      };

    protected internal override Action OnLoaderFinalizeStart
      => () => _writeMessage($"Starting the Process of Finalizing the XBam Loader for Universe: {_loader.Universe.Key}.", "Loader");

    protected internal override Action OnLoaderFinalizeComplete
      => () => {
        _overallStepsRemaining = 0;

        _writeMessage($"Finalized the XBam Loader for Universe: {_loader.Universe.Key}.", "Loader");
      };

    protected internal override Action OnLoaderIsFinished
      => () => {
        _overallStepsRemaining = null;

        _writeMessage($"XBam Loader for Universe: {_loader.Universe.Key} Finished and is now Sealed.", "Loader", verboseNonErrorText:
          $"Enumerations Loaded:"
            + string.Join($"\n\t - ", _loader.Universe.Enumerations.ByType.SelectMany(bt => bt.Value))
          + $"\nArchetypes Loaded:"
            + string.Join($"\n\t - ", _loader.Universe.Archetypes.All.All)
          + $"\nModel Types Loaded:"
            + string.Join($"\n\t - ", _loader.Universe.Models.All.Select(t => t.FullName))
          + $"\nComponent Types Loaded:"
            + string.Join($"\n\t - ", _loader.Universe.Components.All.Select(t => t.FullName))
        );
      };

    protected internal override Action<Archetype> OnUnloadArchetype
      => archetype => {
        _writeMessage($"Unloaded Archetype of Type:{archetype.GetType().FullName}.", "UnLoader");
      };

    protected internal override Action<MemberInfo, JsonProperty> OnLoaderModelJsonPropertyCreationComplete
      => (member, jsonProp) => {
        if (VerboseModeForNonErrors) {
          _writeMessage($"Added Json Property For Serialization: {jsonProp.PropertyName}, to Model: {member.DeclaringType.Name}.", "Serializer");
        }
      };
  }
}
