
  using System;
  using System.Collections;
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
  /*
  /// <summary>
  /// A two way map
  /// Source: https://github.com/farlee2121/BidirectionalMap/blob/main/BidirectionalMap/BiMap.cs
  /// </summary>
  public class Map<TForwardKey, TReverseKey> : IEnumerable<KeyValuePair<TForwardKey, TReverseKey>> {
    public Indexer<TForwardKey, TReverseKey> Forward { get; private set; } = new Indexer<TForwardKey, TReverseKey>();
    public Indexer<TReverseKey, TForwardKey> Reverse { get; private set; } = new Indexer<TReverseKey, TForwardKey>();

    const string DuplicateKeyErrorMessage = "";

    public Map() {}

    public Map(IDictionary<TForwardKey, TReverseKey> oneWayMap) {
      Forward = new Indexer<TForwardKey, TReverseKey>(oneWayMap);
      var reversedOneWayMap = oneWayMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
      Reverse = new Indexer<TReverseKey, TForwardKey>(reversedOneWayMap);
    }

    public void Add(TForwardKey t1, TReverseKey t2) {
      if(Forward.ContainsKey(t1))
        throw new ArgumentException(DuplicateKeyErrorMessage, nameof(t1));
      if(Reverse.ContainsKey(t2))
        throw new ArgumentException(DuplicateKeyErrorMessage, nameof(t2));

      Forward.Add(t1, t2);
      Reverse.Add(t2, t1);
    }

    public bool Remove(TForwardKey forwardKey) {
      if(Forward.ContainsKey(forwardKey) == false)
        return false;
      var reverseKey = Forward[forwardKey];
      bool success;
      if(Forward.Remove(forwardKey)) {
        if(Reverse.Remove(reverseKey)) {
          success = true;
        }
        else {
          Forward.Add(forwardKey, reverseKey);
          success = false;
        }
      }
      else {
        success = false;
      }

      return success;
    }

    public int Count() {
      return Forward.Count();
    }

    IEnumerator<KeyValuePair<TForwardKey, TReverseKey>> IEnumerable<KeyValuePair<TForwardKey, TReverseKey>>.GetEnumerator() {
      return Forward.GetEnumerator();
    }

    public IEnumerator GetEnumerator() {
      return Forward.GetEnumerator();
    }

    /// <summary>
    /// Publically read-only lookup to prevent inconsistent state between forward and reverse map lookups
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public class Indexer<Key, Value> : IEnumerable<KeyValuePair<Key, Value>> {
      private IDictionary<Key, Value> _dictionary;

      public Indexer() {
        _dictionary = new Dictionary<Key, Value>();
      }
      public Indexer(IDictionary<Key, Value> dictionary) {
        _dictionary = dictionary;
      }
      public Value this[Key index] {
        get {
          return _dictionary[index];
        }
      }

      public bool TryToGet(Key key, out Value value) 
        => _dictionary.TryGetValue(key, out value);

      public static implicit operator Dictionary<Key, Value>(Indexer<Key, Value> indexer) {
        return new Dictionary<Key, Value>(indexer._dictionary);
      }

      internal void Add(Key key, Value value) {
        _dictionary.Add(key, value);
      }

      internal bool Remove(Key key) {
        return _dictionary.Remove(key);
      }

      internal int Count() {
        return _dictionary.Count;
      }

      public bool ContainsKey(Key key) {
        return _dictionary.ContainsKey(key);
      }

      public IEnumerable<Key> Keys {
        get {
          return _dictionary.Keys;
        }
      }

      public IEnumerable<Value> Values {
        get {
          return _dictionary.Values;
        }
      }

      /// <summary>
      /// Deep copy lookup as a dictionary
      /// </summary>
      /// <returns></returns>
      public Dictionary<Key, Value> ToDictionary() {
        return new Dictionary<Key, Value>(_dictionary);
      }

      public IEnumerator<KeyValuePair<Key, Value>> GetEnumerator() {
        return _dictionary.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator() {
        return _dictionary.GetEnumerator();
      }
    }
  }*/
}
