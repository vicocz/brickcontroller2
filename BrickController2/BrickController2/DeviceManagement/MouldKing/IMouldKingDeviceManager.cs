using System;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// interface for MouldKingMDeviceanager devices
/// </summary>
public interface IMouldKingDeviceManager
{
    ReadOnlyMemory<byte> GetAppId();
}
