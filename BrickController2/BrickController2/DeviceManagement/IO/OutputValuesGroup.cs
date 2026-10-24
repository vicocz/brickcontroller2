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
    private bool _forceSend; // true until the first successful commit after Initialize()

    private readonly Lock _outputLock = new();

    private int _sendAttemptsLeft;

    public OutputValuesGroup(int channelCount)
    {
        _outputValues = new TValue[channelCount];
        _commitedOutputValues = new TValue[channelCount];
        _values = new TValue[channelCount];
        _sendAttemptsLeft = 0;
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

    public void Initialize()
    {
        lock (_outputLock)
        {
            // reset all values
            _outputValues.AsSpan().Clear();
            _commitedOutputValues.AsSpan().Fill(TValue.One);
            _values.AsSpan().Clear();
            // enable sending for the first round
            _forceSend = true;
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }
    }

    public void Clear()
    {
        lock (_outputLock)
        {
            // reset all values
            _outputValues.AsSpan().Clear();
            _commitedOutputValues.AsSpan().Clear();
            _values.AsSpan().Clear();
            _forceSend = false;
            _sendAttemptsLeft = 0;
        }
    }

    public bool TryGetValues(out ReadOnlySpan<TValue> values) => TryGetValuesCore(out values, out _);

    private bool TryGetValuesCore(out ReadOnlySpan<TValue> values, out bool forced)
    {
        bool sendAttemptsLeft;
        lock (_outputLock)
        {
            // copy values to working copy
            _outputValues.CopyTo(_values.AsSpan());
            forced = _forceSend;
            sendAttemptsLeft = _sendAttemptsLeft > 0;
            _sendAttemptsLeft = sendAttemptsLeft ? --_sendAttemptsLeft : 0;
        }

        values = _values;
        return sendAttemptsLeft && (forced || !values.SequenceEqual(_commitedOutputValues));
    }

    public bool TryGetChanges(out IReadOnlyCollection<KeyValuePair<int, TValue>> changes)
    {
        if (!TryGetValuesCore(out var values, out var forced) || values.IsEmpty)
        {
            changes = [];
            return false;
        }

        // prebuild collection of changes (everything is a change in the forced round)
        changes = [.. _values
        .Select((value, index) => new KeyValuePair<int, TValue>(index, value))
        .Where(x => forced || !x.Value.Equals(_commitedOutputValues[x.Key]))];

        // optimize if absolutely all values are homogenous
        if (changes.Count == values.Length && values.Count(values[0]) == values.Length)
        {
            changes = [new KeyValuePair<int, TValue>(int.MaxValue, values[0])];
        }

        return true;
    }

    public void Commit()
    {
        // reset attempts due to success
        lock (_outputLock)
        {
            // store as last applied values
            _values.CopyTo(_commitedOutputValues.AsSpan());

            _forceSend = false;

            // do it conditionally
            if (_sendAttemptsLeft != MAX_SEND_ATTEMPTS)
            {
                _sendAttemptsLeft = 0;
            }
        }
    }
}
