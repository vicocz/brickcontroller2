using System;
using System.Collections.Generic;
using BrickController2.PlatformServices.BluetoothLE;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BrickController2.Windows.PlatformServices.BluetoothLE
{
    internal class BleGattService : IGattService
    {
        public BleGattService(GattDeviceService bluetoothGattService, IEnumerable<BleGattCharacteristic> characteristics)
        {
            BluetoothGattService = bluetoothGattService;
            Characteristics = characteristics;
        }
        
        public GattDeviceService BluetoothGattService { get; }
        public Guid Uuid => BluetoothGattService.Uuid;
        public IEnumerable<IGattCharacteristic> Characteristics { get; }
    }
}