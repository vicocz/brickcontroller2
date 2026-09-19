using System;

namespace BrickController2.DeviceManagement.JieStar;

/// <summary>
/// Interface for JieStarDeviceManager.
/// </summary>
public interface IJieStarDeviceManager
{
    ReadOnlyMemory<byte> GetAppId();
}
