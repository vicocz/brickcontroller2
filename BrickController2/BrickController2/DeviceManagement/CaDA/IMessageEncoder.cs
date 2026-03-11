using System;

namespace BrickController2.DeviceManagement.CaDA;

public interface IMessageEncoder
{
    /// <summary>
    /// Encode the control data to a byte array which can be sent to the device.
    /// </summary>
    /// <param name="values">Current set of values to encode message for</param>
    /// <param name="connectDevice">Whether the message is for connecting to the device.</param>
    /// <returns>Encoded byte array.</returns>
    byte[] Encode(ReadOnlySpan<Half> values, bool connectDevice = false);

    /// <summary>
    /// Initialize encoder state if necessary.
    /// </summary>
    void Initialize();
}
