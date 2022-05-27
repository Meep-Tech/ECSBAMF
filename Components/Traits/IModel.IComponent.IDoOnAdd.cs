using System;

namespace Meep.Tech.Data {
  public partial interface IModel {

    public partial interface IComponent {

      /// <summary>
      /// Interface indicating this component should do something when added to a model.
      /// </summary>
      public interface IDoOnAdd {

        /// <summary>
        /// Executed when this is added to a model.
        /// </summary>
        internal protected void ExecuteWhenAdded(IModel model);
      }
    }
  }
}
