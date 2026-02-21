using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.CaDA;

/// <summary>
/// Manager for CaDA devices
/// </summary>
public interface ICaDADeviceManager : IBluetoothLEAdvertiserDeviceScanInfo, IBluetoothLEDeviceManager
{
    public ushort AppId { get; }
}