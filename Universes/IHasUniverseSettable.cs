using System;
using System.Collections.Generic;
using System.Text;

namespace Meep.Tech.Data.Universes {
  internal interface IHasUniverseSettable {
    Universe Universe {
      get;
      internal set;
    }
  }
}
