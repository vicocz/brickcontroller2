using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickController2.Droid.PlatformServices.BluetoothLE;

internal class BluetoothLEDevice_SdkIKD : BluetoothLEDevice
{
    public BluetoothLEDevice_SdkIKD(Context context, BluetoothAdapter bluetoothAdapter, string address)
        : base(context, bluetoothAdapter, address)
    {
    }

    public override void OnCharacteristicRead(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic, [GeneratedEnum] GattStatus status)
    {
        lock (_lock)
        {
#pragma warning disable CA1422 // Validate platform compatibility
            _readCompletionSource?.TrySetResult(characteristic?.GetValue());
#pragma warning restore CA1422 // Validate platform compatibility
        }
    }

    public override void OnCharacteristicWrite(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic, [GeneratedEnum] GattStatus status)
    {
        lock (_lock)
        {
            _writeCompletionSource?.TrySetResult(status == GattStatus.Success);
        }
    }

    public override void OnCharacteristicChanged(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic)
    {
        // skip if there is nothing to publish
        if (characteristic?.Uuid is null || _onCharacteristicChanged is null)
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        lock (_lock)
        {
            var guid = characteristic.Uuid.ToGuid();
#pragma warning disable CA1422 // Validate platform compatibility
            var data = characteristic.GetValue();
#pragma warning restore CA1422 // Validate platform compatibility

            if (data is not null)
            {
                _onCharacteristicChanged.Invoke(guid, data);
            }
        }

        System.Diagnostics.Debug.WriteLine("OnCharacteristicChanged: " + stopwatch.Elapsed);
    }
}
