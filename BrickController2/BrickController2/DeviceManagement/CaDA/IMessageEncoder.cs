using System;

namespace BrickController2.DeviceManagement.CaDA;

public interface IMessageEncoder
{
    /// <summary>
    /// Encode the control data to a byte array which can be sent to the device.
    /// </summary>
    /// <param name="values">Current set of values to encode message for</param>
    /// <param name="connect">Whether the message is for connecting to the device. If true, the message will contain additional information about the device and app.</param>
    /// <returns>Encoded byte array</returns>
    ReadOnlySpan<byte> Encode(ReadOnlySpan<Half> values, bool connect = false);

    void Initialize();
}
