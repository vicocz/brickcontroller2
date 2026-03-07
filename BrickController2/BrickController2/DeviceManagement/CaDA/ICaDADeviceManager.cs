using BrickController2.PlatformServices.BluetoothLE;
using System;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// Manager for CaDA devices
/// </summary>
public interface ICaDADeviceManager : IBluetoothLEAdvertiserDeviceScanInfo, IBluetoothLEDeviceManager
{
    ReadOnlyMemory<byte> GetAppId();
}
