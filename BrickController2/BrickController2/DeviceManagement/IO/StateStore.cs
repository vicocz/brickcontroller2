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

    /// <summary>
    /// Atomically applies <paramref name="updater"/> to the current state and returns
    /// the state that existed <em>before</em> the update (the consumed snapshot).
    /// This prevents a race where a concurrent write between a Get() and a separate
    /// Update() call causes the new write to be silently cleared.
    /// </summary>
    public TValue Exchange(TKey key, Func<TValue, TValue> updater)
    {
        while (true)
        {
            var old = Get(key);
            var next = updater(old);

            // If the key is already present, do a compare-and-swap.
            if (_states.TryUpdate(key, next, old))
                return old;

            // Key was absent; race to insert the computed next value.
            if (_states.TryAdd(key, next))
                return _default;

            // Another thread beat us - retry.
        }
    }

    /// <summary>Clears all persisted states.</summary>
    public void Clear() => _states.Clear();

    /// <summary>Returns the maximum value of a projection over all stored states, or default if empty.</summary>
    public TResult Max<TResult>(Func<TValue, TResult> selector) where TResult : struct
        => _states.Values.Select(selector)
            .DefaultIfEmpty(selector(_default))
            .Max();
}
