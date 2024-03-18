using BrickController2.PlatformServices.BluetoothLE;
using Tizen.Network.Bluetooth;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

internal class GattService : IGattService
{
    public GattService(BluetoothGattService bluetoothGattService, IEnumerable<GattCharacteristic> characteristics)
    {
        BluetoothGattService = bluetoothGattService;
        Uuid = Guid.Parse(bluetoothGattService.Uuid);
        Characteristics = characteristics;
    }

    public BluetoothGattService BluetoothGattService { get; }
    public Guid Uuid { get; }
    public IEnumerable<IGattCharacteristic> Characteristics { get; }
}