using System;

namespace Meep.Tech.Data.Configuration {
  public partial class Loader {

    /// <summary>
    /// Exeption thrown when you fail to initialize or finalize an archetype. This will cause the loader to retry for things like missing dependencies that haven't loaded yet:
    /// </summary>
    public class FailedToConfigureNewArchetypeException : InvalidOperationException {
      public FailedToConfigureNewArchetypeException(string message) : base(message) { }
      public FailedToConfigureNewArchetypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you fail to initialize an archetype because another archetype is missing. This will cause the loader to retry for things like missing dependencies that haven't loaded yet:
    /// </summary>
    public class MissingArchetypeDependencyException : FailedToConfigureNewArchetypeException {
      public MissingArchetypeDependencyException(string message) : base(message) { }
      public MissingArchetypeDependencyException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you cannot to initialize an archetype. This will cause the loader to stop trying for this archetype and mark it as failed completely:
    /// </summary>
    public class CannotInitializeArchetypeException : InvalidOperationException {
      public CannotInitializeArchetypeException(string message) : base(message) { }
      public CannotInitializeArchetypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when you cannot to initialize an model type, or build a test model for an archetype.
    /// </summary>
    public class CannotInitializeModelException : InvalidOperationException {
      public CannotInitializeModelException(string message) : base(message) { }
      public CannotInitializeModelException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exeption thrown when a component is missing, and a Model or Archetype can't be built because of it
    /// </summary>
    public class MissingComponentDependencyException : InvalidOperationException {
      public MissingComponentDependencyException(string message) : base(message) { }
      public MissingComponentDependencyException(string message, Exception innerException) : base(message, innerException) { }
    }
  }
}