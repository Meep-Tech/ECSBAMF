using Meep.Tech.Data;
using System.Collections.Generic;

namespace Meep.Tech.Messaging {
  public abstract partial class Event {

    /// <summary>
    /// A tag for a nevent. Tags work like channels that can be listened into by observers.
    /// </summary>
    public class Tag : Enumeration<Tag> {
      static readonly Dictionary<string, Tag> _withExtraContext = new();

      /// <summary>
      /// Make a new tag.
      /// </summary>
      public Tag(string key, Universe universe = null)
        : base(key, universe) { }

      /// <summary>
      /// Make a version of this tag with some required extra context.
      /// Can be used to make specific events like 'level-up|[CHARACTERID]' vs just 'level-up'
      /// </summary>
      public Tag WithExtraContext(params string[] extraContexts) {
        string key = ExternalId as string + string.Join('|', extraContexts);
        return _withExtraContext.TryGetValue(key, out Tag existing)
          ? existing
          : (_withExtraContext[key] = new(key, Universe));
      }
    }
  }
}