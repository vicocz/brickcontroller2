using System;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Interface for MouldKingDeviceManager.
/// </summary>
public interface IMouldKingDeviceManager
{
    ReadOnlyMemory<byte> GetAppId();
}
