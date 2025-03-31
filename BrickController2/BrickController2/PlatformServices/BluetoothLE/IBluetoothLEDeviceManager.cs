using BrickController2.DeviceManagement;

namespace BrickController2.PlatformServices.BluetoothLE;

public interface IBluetoothLEDeviceManager
{
    bool TryGetDevice(string manufacturerId, byte[] manufacturerData, out DeviceType deviceType, ref string deviceName, ref string deviceAddress);
}
