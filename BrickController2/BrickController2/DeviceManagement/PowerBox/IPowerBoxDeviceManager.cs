using System;

namespace BrickController2.DeviceManagement.PowerBox;

/// <summary>
/// Interface for PowerBoxDeviceManager.
/// </summary>
public interface IPowerBoxDeviceManager
{
    ReadOnlyMemory<byte> GetAppId();
}
