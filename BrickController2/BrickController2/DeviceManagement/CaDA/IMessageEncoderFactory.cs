using System;

namespace BrickController2.DeviceManagement.CaDA;

public interface IMessageEncoderFactory
{
    /// <summary>
    /// Create a new message encoder for the specified CaDA device.
    /// </summary>
    /// <param name="deviceData">Device data</param>
    /// <returns>New message encoder instance</returns>
    IMessageEncoder Create(ReadOnlySpan<byte> deviceData);
}
