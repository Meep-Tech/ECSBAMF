using System;

namespace Meep.Tech.Data {

  public partial class Model {

    /// <summary>
    /// A set of pre-serialized packaged model data.
    /// TODO: can this be removed with EF?
    /// </summary>
    public struct SerializedData {

      public IModel Deserialize() {
        throw new NotImplementedException();
      }
      
      public IModel DeserializeAs<TType>() where TType : IModel {
        throw new NotImplementedException();
      }
    }
  
  }
}