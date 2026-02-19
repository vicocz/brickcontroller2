using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Droid.PlatformServices.BluetoothLE
{
    internal class GattService : IGattService
    {
        private readonly IReadOnlySet<Guid> _characteristics;

        public GattService(BluetoothGattService bluetoothGattService)
        {
            BluetoothGattService = bluetoothGattService;
            _characteristics = new HashSet<Guid>(bluetoothGattService.Characteristics!.Select(ch => ch.Uuid!.ToGuid()));
        }
        
        public BluetoothGattService BluetoothGattService { get; }
        public Guid Uuid => BluetoothGattService.Uuid!.ToGuid();

        public bool ContainsCharacteristic(Guid characteristicUuid) => _characteristics.Contains(characteristicUuid);
    }
}