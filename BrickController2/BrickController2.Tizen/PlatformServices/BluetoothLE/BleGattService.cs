using Tizen.Network.Bluetooth;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

internal class BleGattService : IGattService
{
    public BleGattService(BluetoothGattService bluetoothGattService, IEnumerable<BleGattCharacteristic> characteristics)
    {
        BluetoothGattService = bluetoothGattService;
        Characteristics = characteristics;
        Uuid = Guid.Parse(bluetoothGattService.Uuid);
    }

    public BluetoothGattService BluetoothGattService { get; }
    public Guid Uuid { get; }
    public IEnumerable<IGattCharacteristic> Characteristics { get; }
}