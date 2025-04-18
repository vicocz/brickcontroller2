using BrickController2.DeviceManagement;
using BrickController2.Helpers;

namespace BrickController2.PlatformServices.BluetoothLE;

public interface IBluetoothLEDeviceManager
{
    bool TryGetDevice(string manufacturerId, byte[] manufacturerData, ref FoundDevice foundDevice);
}
