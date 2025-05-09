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

    private readonly Lock _outputLock = new();

    private int _sendAttemptsLeft;

    public OutputValuesGroup(int channelCount)
    {
        _outputValues = new TValue[channelCount];
        _commitedOutputValues = new TValue[channelCount];
        _values = new TValue[channelCount];
        _sendAttemptsLeft = 0;
    }

    public void SetOutput(int channel, TValue value)
    {
        lock (_outputLock)
        {
            if (!_outputValues[channel].Equals(value))
            {
                _outputValues[channel] = value;
                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
            }
        }        
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
            _sendAttemptsLeft = MAX_SEND_ATTEMPTS;
        }
    }

    /// <summary>
    /// Try to get the output values to be sent, if there was any change or last application was not succcessfull
    /// </summary>
    /// <param name="values">Set of all values</param>
    /// <returns>true there is any reason to apply values</returns>
    public bool TryGetValues(out ReadOnlySpan<TValue> values)
    {
        bool sendAttemptsLeft;
        lock (_outputLock)
        {
            // copy values to working copy
            _outputValues.CopyTo(_values.AsSpan());
            sendAttemptsLeft = _sendAttemptsLeft > 0;
            _sendAttemptsLeft = sendAttemptsLeft ? --_sendAttemptsLeft : 0;
        }

        values = _values;
        return sendAttemptsLeft && !values.SequenceEqual(_commitedOutputValues);
    }

    /// <summary>
    /// Try to get the output values to be sent, if there was any change or last application was not succcessfull
    /// </summary>
    /// <param name="changes">Collection of changes</param>
    /// <returns>true there is any reason to apply changes</returns>
    public bool TryGetChanges(out IReadOnlyCollection<KeyValuePair<int,TValue>> changes)
    {
        if (!TryGetValues(out var values) || values.IsEmpty)
        {
            changes = [];
            return false;
        }

        // prebuild collection of changes
        changes = [.. _values
            .Select((value, index) => new KeyValuePair<int, TValue>(index, value))
            .Where(x => !x.Value.Equals(_commitedOutputValues[x.Key]))];

        // optimize if absolutely all values are homogenous
        if (changes.Count == values.Length && values.Count(values[0]) == values.Length)
        {
            changes = [new KeyValuePair<int, TValue>(int.MaxValue, values[0])];
        }

        return true;
    }

    /// <summary>
    /// Confirm that the values have been sent and applied.
    /// </summary>
    public void Commmit()
    {
        // store as last applied values
        _values.CopyTo(_commitedOutputValues.AsSpan());

        // reset attemps due to success
        lock (_outputLock)
        {
            // do it conditionally
            if (_sendAttemptsLeft != MAX_SEND_ATTEMPTS)
            {
                _sendAttemptsLeft = 0;
            }
        }
    }
}
