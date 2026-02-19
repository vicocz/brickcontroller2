using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

internal class BleGattService : IGattService, IDisposable
{
    private readonly HashSet<Guid> _characteristics;

    public BleGattService(GattDeviceService bluetoothGattService, IEnumerable<BleGattCharacteristic> characteristics)
    {
        BluetoothGattService = bluetoothGattService;
        _characteristics = characteristics.Select(ch => ch.Uuid).ToHashSet();
    }

    public GattDeviceService BluetoothGattService { get; }
    public Guid Uuid => BluetoothGattService.Uuid;

    private bool disposed;

    public void Dispose()
    {
        try
        {
            if (!disposed)
            {
                disposed = true;
                BluetoothGattService.Dispose();
            }
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public bool ContainsCharacteristic(Guid characteristicUuid) => _characteristics.Contains(characteristicUuid);
}