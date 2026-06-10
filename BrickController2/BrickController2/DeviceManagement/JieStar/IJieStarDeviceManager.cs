using System;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// Manager for JIESTAR devices
/// </summary>
public interface IJieStarDeviceManager
{
    ReadOnlyMemory<byte> GetAppId();
}
