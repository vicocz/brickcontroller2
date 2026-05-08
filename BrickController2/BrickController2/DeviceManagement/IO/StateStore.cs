using System;
using System.Collections.Concurrent;
using System.Linq;

namespace BrickController2.DeviceManagement.IO;

/// <summary>
/// Thread-safe, key-indexed store for per-key state structs.
/// Supports atomic read, write, and functional update.
/// </summary>
internal class StateStore<TKey, TValue>
    where TKey : notnull
    where TValue : struct
{
    private readonly ConcurrentDictionary<TKey, TValue> _states = new();
    private readonly TValue _default;

    public StateStore(TValue initialState = default)
    {
        _states = new();
        _default = initialState;
    }

    public int Count => _states.Count;

    /// <summary>Returns the current state for the given key or the default state if the key does not exist.</summary>
    public TValue Get(TKey key) => _states.TryGetValue(key, out var value) ? value : _default;

    /// <summary>Removes the current state for the given key.</summary>
    public bool Remove(TKey key) => _states.TryRemove(key, out var _);

    /// <summary>Upsert the state for the given key.</summary>
    public void Set(TKey key, TValue state = default)
    {
        _states[key] = state;
    }

    /// <summary>
    /// Atomically updates the state for the given key using the provided updater function.
    /// Returns the new state.
    /// </summary>
    public TValue Update(TKey key, Func<TValue, TValue> updater) => _states.AddOrUpdate(key, (k) => updater(_default), (k, o) => updater(o));

    /// <summary>Clears all persisted states.</summary>
    public void Clear() => _states.Clear();

    /// <summary>Returns the maximum value of a projection over all stored states, or default if empty.</summary>
    public TResult? Max<TResult>(Func<TValue, TResult> selector) => _states.Values.Max(x => selector(x));
}
