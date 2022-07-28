namespace Meep.Tech.Data.Universes {
  internal interface IHasUniverseSettable {
    Universe Universe {
      get;
      internal set;
    }
  }
}
