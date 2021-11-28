using System;

namespace Meep.Tech.Data {

  public partial class Model {

    /// <summary>
    /// A set of pre-serialized packaged model data.
    /// TODO: can this be removed with EF?
    /// </summary>
    public struct SerializedData {

      public IModel deserialize() {
        throw new NotImplementedException();
      }
      
      public IModel deserializeAs<TType>() where TType : IModel {
        throw new NotImplementedException();
      }
    }
  
  }
}