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
    private readonly bool[] _changedValues; // flags indicating which channels have changed since last commit

    private readonly Lock _outputLock = new();

    private int _sendAttemptsLeft;

    public OutputValuesGroup(int channelCount)
    {
        _outputValues = new TValue[channelCount];
        _commitedOutputValues = new TValue[channelCount];
        _values = new TValue[channelCount];
        _changedValues = new bool[channelCount];
        _sendAttemptsLeft = 0;
    }

    public bool SetOutput(int channel, TValue value)
    {
        lock (_outputLock)
        {
            TValue committedValue = _commitedOutputValues[channel];
            TValue outputValue = _outputValues[channel];

            if (!outputValue.Equals(value))
            {
                _outputValues[channel] = value;
                _changedValues[channel] = !committedValue.Equals(value);
                _sendAttemptsLeft = MAX_SEND_ATTEMPTS;

                return true;
            }
            else
            {
                // Keep change tracking aligned with the committed snapshot even if the same
                // value is written again after a prior change was reverted.
                _changedValues[channel] = !outputValue.Equals(committedValue);
                return false;
            }
        }
    }

    public void Initialize()
    {
        lock (_outputLock)
        {
            // reset all values
            _outputValues.AsSpan().Clear();
            // clear committed values and mark all channels as changed so the first send rounds will run
            _commitedOutputValues.AsSpan().Clear();
            _changedValues.AsSpan().Fill(true);
            _values.AsSpan().Clear();
            // enable sending for the first round
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
            _changedValues.AsSpan().Clear();
            _sendAttemptsLeft = 0;
        }
    }

    /// <summary>
    /// Try to get the output values to be sent, if there was any change or last application was not successful
    /// </summary>
    /// <param name="values">Set of all values</param>
    /// <returns>true there is any reason to apply values</returns>
    public bool TryGetValues(out ReadOnlySpan<TValue> values)
    {
        bool sendAttemptsLeft;
        bool anyChanged;
        bool equalToCommitted;

        lock (_outputLock)
        {
            // copy values to working copy
            _outputValues.CopyTo(_values.AsSpan());
            sendAttemptsLeft = _sendAttemptsLeft > 0;
            _sendAttemptsLeft = sendAttemptsLeft ? --_sendAttemptsLeft : 0;

            // Recompute stale change flags against the committed snapshot so a channel
            // set back to its committed value is not treated as still changed.
            anyChanged = _changedValues.Any(changed => changed)
                || _values.Select((value, index) => !value.Equals(_commitedOutputValues[index])).Any(changed => changed);
            equalToCommitted = _values.SequenceEqual(_commitedOutputValues);
        }

        values = _values;

        // Send if there are any changed channels or the current working values differ from committed values.
        return sendAttemptsLeft && (anyChanged || !equalToCommitted);
    }
    /// <summary>
    /// Try to get the output values to be sent, if there was any change or last application was not successful
    /// </summary>
    /// <param name="changes">Collection of changes</param>
    /// <returns>true there is any reason to apply changes</returns>
    public bool TryGetChanges(out IReadOnlyCollection<KeyValuePair<int, TValue>> changes)
    {
        if (!TryGetValues(out var values) || values.IsEmpty)
        {
            changes = [];
            return false;
        }

        // snapshot changed flags and committed values under lock to avoid races
        bool[] changedSnapshot;
        TValue[] committedSnapshot;

        lock (_outputLock)
        {
            changedSnapshot = _changedValues.ToArray();
            committedSnapshot = _commitedOutputValues.ToArray();
        }

        // prebuild collection of changes based on the snapshot
        changes = [.. _values
            .Select((value, index) => new KeyValuePair<int, TValue>(index, value))
            .Where(x => changedSnapshot[x.Key] || !x.Value.Equals(committedSnapshot[x.Key]))];

        // optimize if absolutely all values are homogenous
        if (changes.Count == values.Length)
        {
            TValue[] arr = values.ToArray(); // materialize span so lambdas don't capture a ref local
            if (arr.All(v => v.Equals(arr[0])))
            {
                changes = [new KeyValuePair<int, TValue>(int.MaxValue, arr[0])];
            }
        }

        return true;
    }

    /// <summary>
    /// Confirm that the values have been sent and applied.
    /// </summary>
    public void Commit()
    {
        lock (_outputLock)
        {
            // store as last applied values
            _values.CopyTo(_commitedOutputValues.AsSpan());
            // reset changed flags since committed reflects current values
            _changedValues.AsSpan().Clear();

            // reset attempts due to success
            // do it conditionally
            if (_sendAttemptsLeft != MAX_SEND_ATTEMPTS)
            {
                _sendAttemptsLeft = 0;
            }
        }
    }
}
