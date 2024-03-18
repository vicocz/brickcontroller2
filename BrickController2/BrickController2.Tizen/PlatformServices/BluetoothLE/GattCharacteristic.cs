using BrickController2.PlatformServices.BluetoothLE;
using Tizen.Network.Bluetooth;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

internal class GattCharacteristic : IGattCharacteristic
{
    public GattCharacteristic(BluetoothGattCharacteristic bluetoothGattCharacteristic)
    {
        BluetoothGattCharacteristic = bluetoothGattCharacteristic;
        Uuid = Guid.Parse(bluetoothGattCharacteristic.Uuid);
    }

    public BluetoothGattCharacteristic BluetoothGattCharacteristic { get; }
    public Guid Uuid { get; }
}