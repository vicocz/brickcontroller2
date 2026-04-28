using System;
using System.Threading;

namespace BrickController2.DeviceManagement.IO;

/// <summary>
/// Thread-safe, channel-indexed store for per-channel state structs.
/// Supports atomic read, write, and functional update.
/// </summary>
internal sealed class ChannelStateStore<T> where T : struct
{
    private readonly T[] _states;
    private readonly Lock _lock = new();

    public ChannelStateStore(int channelCount, T initialState = default)
    {
        _states = new T[channelCount];
        _states.AsSpan().Fill(initialState);
    }

    public int ChannelCount => _states.Length;

    /// <summary>Returns the current state for the given channel.</summary>
    public T Get(int channel)
    {
        lock (_lock) return _states[channel];
    }

    /// <summary>Replaces the state for the given channel.</summary>
    public void Set(int channel, T state = default)
    {
        lock (_lock) _states[channel] = state;
    }

    /// <summary>
    /// Atomically updates the state for the given channel using the provided updater function.
    /// Returns the new state.
    /// </summary>
    public T Update(int channel, Func<T, T> updater)
    {
        lock (_lock)
        {
            var updated = updater(_states[channel]);
            _states[channel] = updated;
            return updated;
        }
    }

    /// <summary>Resets all channels to the given state.</summary>
    public void ResetAll(T state = default)
    {
        lock (_lock) _states.AsSpan().Fill(state);
    }

    /// <summary>Resets all channels using a per-channel factory.</summary>
    public void ResetAll(Func<int, T> factory)
    {
        lock (_lock)
        {
            for (int i = 0; i < _states.Length; i++)
                _states[i] = factory(i);
        }
    }
}
