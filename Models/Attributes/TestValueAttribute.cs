using System;

namespace Meep.Tech.Data {

  /// <summary>
  /// An attribute that adds a value to the DefaultTestParams field of an archetype by default.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueAttribute : Attribute {

    /// <summary>
    /// The value of the attribute
    /// </summary>
    public virtual object Value {
      get => _value;
      init => _value = value;
    } internal object _value;

    /// <summary>
    /// Set the DefaultTestParam value
    /// </summary>
    /// <param name="value"></param>
    public TestValueAttribute(object value = null) {
      Value = value;
    }
  }

  /// <summary>
  /// An attribute that activates a default object of the type as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueIsNewAttribute : TestValueAttribute {

    /// <summary>
    /// Set the test value of this field to 'new()';
    /// </summary>
    public TestValueIsNewAttribute() : base() { }
  }

  /// <summary>
  /// An attribute that gets a value from a local member as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class GetTestValueFromMemberAttribute : TestValueAttribute {

    /// <summary>
    /// The name of the method to use to get the value.
    /// </summary>
    public string MethodName { 
      get;
      init;
    }

    /// <summary>
    /// Set the test value of this field to 'new()';
    /// </summary>
    public GetTestValueFromMemberAttribute(string methodName) : base() {
      MethodName = methodName;
    }
  }

  /// <summary>
  /// An attribute that activates an empty enum of the seired type as a the DefaultTestParams value for this field.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, Inherited = true)]
  public class TestValueIsEmptyEnumerableAttribute : TestValueAttribute {

    /// <summary>
    /// Set the test value of this field to 'Enumerable.Empty<T>()';
    /// </summary>
    public TestValueIsEmptyEnumerableAttribute() : base() { }
  }
}
