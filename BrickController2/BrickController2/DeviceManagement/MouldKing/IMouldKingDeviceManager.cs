using System;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Interface definition for MouldKing specific PlatformService
/// </summary>
public interface IMouldKingDeviceManager
{
    ReadOnlyMemory<byte> GetAppId();
}
