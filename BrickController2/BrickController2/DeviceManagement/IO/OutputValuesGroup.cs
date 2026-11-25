using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace BrickController2.DeviceManagement.IO;

public class OutputValuesGroup<TValue> where TValue : struct, IEquatable<TValue>, INumber<TValue>
{
    private const int MAX_SEND_ATTEMPTS = 5;

    private readonly TValue[] _outputValues; // current snapshot of values, not yet applied
    private readonly TValue[] _commitedOutputValues; // values applied
    private readonly TValue[] _values; // working copy of values, valid until commited

    private readonly Lock _outputLock;

    private int _sendAttemptsLeft;
    private bool _hasCommitted;

    public OutputValuesGroup(int channelCount)
        : this(channelCount, new Lock())
    {
    }

    public OutputValuesGroup(int channelCount, Lock outputLock)
    {
        _outputLock = outputLock ?? new Lock();
        _outputValues = new TValue[channelCount];
        _commitedOutputValues = new TValue[channelCount];
        _values = new TValue[channelCount];
    }

    public bool SetOutput(int channel, TValue value)
    {
        lock (_outputLock)
        {
            if (!_outputValues[channel].Equals(value))
            {
                _outputValues[channel] = value;
                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

                return true;
            }
        }
        return false;
    }

    public void Initialize() => Reset(MAX_SEND_ATTEMPTS, false);

    public void Clear() => Reset(0, true);

    /// <summary>
    /// Try to get the output values to be sent, if there was any change or last application was not successful
    /// </summary>
    /// <param name="values">Set of all values</param>
    /// <returns>true there is any reason to apply values</returns>
    public bool TryGetValues(out ReadOnlySpan<TValue> values) => TryGetValues(out values, out _);

    /// <summary>
    /// Try to get the output values to be sent, if there was any change or last application was not successful
    /// </summary>
    /// <param name="changes">Collection of changes</param>
    /// <returns>true there is any reason to apply changes</returns>
    public bool TryGetChanges(out IReadOnlyList<KeyValuePair<int, TValue>> changes)
    {
        if (!TryGetValues(out var values, out bool hasCommitted) || values.IsEmpty)
        {
            changes = [];
            return false;
        }

        int changeCount = 0;
        bool isHomogenous = true;
        TValue firstValue = _values[0];
        var buffer = new KeyValuePair<int, TValue>[values.Length];

        for (int i = 0; i < _values.Length; i++)
        {
            TValue value = _values[i];

            if (isHomogenous && !value.Equals(firstValue))
            {
                isHomogenous = false;
            }

            if (!hasCommitted || !value.Equals(_commitedOutputValues[i]))
            {
                buffer[changeCount++] = new KeyValuePair<int, TValue>(i, value);
            }
        }

        if (changeCount == 0)
        {
            changes = [];
            return false;
        }

        if (isHomogenous && changeCount == _values.Length)
        {
            buffer[0] = new KeyValuePair<int, TValue>(int.MaxValue, firstValue);
            changes = new ArraySegment<KeyValuePair<int, TValue>>(buffer, 0, 1);
            return true;
        }

        changes = new ArraySegment<KeyValuePair<int, TValue>>(buffer, 0, changeCount);
        return true;
    }

    /// <summary>
    /// Confirm that the values have been sent and applied.
    /// </summary>
    public void Commit()
    {
        // reset attempts due to success
        lock (_outputLock)
        {
            // store as last applied values
            _values.CopyTo(_commitedOutputValues.AsSpan());
            _hasCommitted = true;

            // do it conditionally
            if (_sendAttemptsLeft != MAX_SEND_ATTEMPTS)
            {
                _sendAttemptsLeft = 0;
            }
        }
    }

    private bool TryGetValues(out ReadOnlySpan<TValue> values, out bool hasCommitted)
    {
        bool sendAttemptsLeft;
        lock (_outputLock)
        {
            _outputValues.CopyTo(_values.AsSpan());
            sendAttemptsLeft = _sendAttemptsLeft > 0;
            _sendAttemptsLeft = sendAttemptsLeft ? --_sendAttemptsLeft : 0;

            // Snapshot _hasCommitted inside the lock to ensure memory barrier visibility
            hasCommitted = _hasCommitted;
        }

        values = _values;
        return sendAttemptsLeft && (!hasCommitted || !values.SequenceEqual(_commitedOutputValues));
    }

    private void Reset(int attemptsLeft, bool hasCommitted)
    {
        lock (_outputLock)
        {
            // reset all values
            _outputValues.AsSpan().Clear();
            _commitedOutputValues.AsSpan().Clear();
            _values.AsSpan().Clear();
            _sendAttemptsLeft = attemptsLeft;
            _hasCommitted = hasCommitted;
        }
    }
}
