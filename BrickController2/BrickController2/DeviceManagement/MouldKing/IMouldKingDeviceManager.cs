using System;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Interface definition for MouldKing-specific device manager functionality.
/// </summary>
public interface IMouldKingDeviceManager
{
    ReadOnlyMemory<byte> GetAppId();
}
