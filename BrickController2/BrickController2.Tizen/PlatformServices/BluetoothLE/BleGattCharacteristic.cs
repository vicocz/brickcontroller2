using Tizen.Network.Bluetooth;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

internal class BleGattCharacteristic : IGattCharacteristic
{
    public BleGattCharacteristic(BluetoothGattCharacteristic bluetoothGattCharacteristic)
    {
        BluetoothGattCharacteristic = bluetoothGattCharacteristic;
        Uuid = Guid.Parse(bluetoothGattCharacteristic.Uuid);
    }

    public BluetoothGattCharacteristic BluetoothGattCharacteristic { get; }
    public Guid Uuid { get; }
}