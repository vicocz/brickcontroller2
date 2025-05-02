using BrickController2.DeviceManagement;

namespace BrickController2.PlatformServices.BluetoothLE;

public interface IBluetoothLEDeviceManager
{
    bool TryGetDevice(ScanResult scanResult, out FoundDevice device);
}
