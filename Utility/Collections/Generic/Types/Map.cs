
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meep.Tech.Collections.Generic {

  /// <summary>
  /// A better, home made version of the 2 way map.
  /// </summary>
  public class Map<TForwardKey, TReverseKey> {

    /// <summary>
    /// The forward key value set
    /// </summary>
    public IReadOnlyDictionary<TForwardKey, TReverseKey> Forward
     => _forward; Dictionary<TForwardKey, TReverseKey> _forward;

    /// <summary>
    /// The reversed key value set
    /// </summary>
    public IReadOnlyDictionary<TReverseKey, TForwardKey> Reverse
      => _reverse; Dictionary<TReverseKey, TForwardKey> _reverse;

    public Map() {
      _forward = new Dictionary<TForwardKey, TReverseKey>();
      _reverse = new Dictionary<TReverseKey, TForwardKey>();
    }

    public Map(IEnumerable<KeyValuePair<TForwardKey, TReverseKey>> pairs) {
      _forward = pairs?.ToDictionary(e => e.Key, e => e.Value) ?? new();
      _reverse = pairs?.ToDictionary(e => e.Value, e => e.Key) ?? new();
    }

    public Map(Dictionary<TForwardKey, TReverseKey> forwardsMap) {
      if(forwardsMap is null) {
        _forward = new Dictionary<TForwardKey, TReverseKey>();
      } else
        _forward = forwardsMap;

      _reverse = forwardsMap.ToDictionary(e => e.Value, e => e.Key);
    }

    /// <summary>
    /// Add a pair of keys.
    /// This throws if there is already a key in either collection with the given values.
    /// </summary>
    public void Add(TForwardKey forwardKey, TReverseKey reverseKey) {
      _forward.Add(forwardKey, reverseKey);
      _reverse.Add(reverseKey, forwardKey);
    }

    /// <summary>
    /// Add a pair of keys.
    /// This throws if there is already a key in either collection with the given values.
    /// </summary>
    public void Add(KeyValuePair<TForwardKey, TReverseKey> forwardAndReversePair)
      => Add(forwardAndReversePair.Key, forwardAndReversePair.Value);

    /// <summary>
    /// Add a pair of keys.
    /// This throws if there is already a key in either collection with the given values.
    /// </summary>
    public void Add((TForwardKey forward, TReverseKey reverse) forwardAndReversePair)
      => Add(forwardAndReversePair.forward, forwardAndReversePair.reverse);

    /// <summary>
    /// Update both links to point to eachother, removing any current links for the values.
    /// </summary>
    public void Update(TForwardKey forwardKey, TReverseKey reverseKey) {
      if(Remove(forwardKey)) {
        Add(forwardKey, reverseKey);
      } else
        throw new Exception($"Unknown issue while trying to remove item {forwardKey}::{reverseKey} from Map during Update call.");
    }

    /// <summary>
    /// Update both links to point to eachother, removing any current links for the values.
    /// </summary>
    public void Update(KeyValuePair<TForwardKey, TReverseKey> forwardAndReversePair)
      => Update(forwardAndReversePair.Key, forwardAndReversePair.Value);

    /// <summary>
    /// Update both links to point to eachother, removing any current links for the values.
    /// </summary>
    public void Update((TForwardKey forward, TReverseKey reverse) forwardAndReversePair)
      => Update(forwardAndReversePair.forward, forwardAndReversePair.reverse);

    /// <summary>
    /// Try to remove an entry using the forward key
    /// </summary>
    public bool Remove(TForwardKey forwardKey) {
      if(!Forward.ContainsKey(forwardKey)) {
        return false;
      }

      bool success;
      if(_forward.Remove(forwardKey)) {
        TReverseKey reverseKey = Forward[forwardKey];
        if(_reverse.Remove(reverseKey)) {
          success = true;
        } else {
          _forward.Add(forwardKey, reverseKey);
          success = false;
        }
      } else {
        success = false;
      }

      return success;
    }

    /// <summary>
    /// Try to remove an entry using the forward key
    /// </summary>
    public bool RemoveWithReverseKey(TReverseKey reverseKey) {
      if(!Reverse.ContainsKey(reverseKey)) {
        return false;
      }

      bool success;
      if(_reverse.Remove(reverseKey)) {
        TForwardKey forwardKey = Reverse[reverseKey];
        if(_forward.Remove(forwardKey)) {
          success = true;
        } else {
          _reverse.Add(reverseKey, forwardKey);
          success = false;
        }
      } else {
        success = false;
      }

      return success;
    }

    /// <summary>
    /// The number of entries
    /// </summary>
    public int Count() {
      return Forward.Count();
    }
  }
}
