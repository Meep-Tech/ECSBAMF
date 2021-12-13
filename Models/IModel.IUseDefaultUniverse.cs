using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data {

  public partial interface IModel {

    /// <summary>
    /// Just makes the struct based model or component use the default universe so you don't need to set it yourself
    /// </summary>
    public interface IUseDefaultUniverse : IModel {

      /// <summary>
      /// This can be overriden if you want, but by default, struct based components don't have universe info at hand
      /// </summary>
      Universe IModel.Universe {
        get => Components.DefaultUniverse;
      }
    }
  }
}
