using Android.Bluetooth;
using Android.Content;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.Droid.PlatformServices.BluetoothLE;

internal class BluetoothLEDevice_Sdk33 : BluetoothLEDevice
{
    public BluetoothLEDevice_Sdk33(Context context, BluetoothAdapter bluetoothAdapter, string address)
        : base(context, bluetoothAdapter, address)
    {
    }

    public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value, [GeneratedEnum] GattStatus status)
    {
        lock (_lock)
        {
            if (status == GattStatus.Success)
            {
                _readCompletionSource?.TrySetResult(value);
            }
            else
            {
                _readCompletionSource?.TrySetResult(default);
            }
        }
    }

    public override void OnCharacteristicWrite(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic, [GeneratedEnum] GattStatus status)
    {
        lock (_lock)
        {
            _writeCompletionSource?.TrySetResult(status == GattStatus.Success);
        }
    }

    public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, byte[] value)
    {
        // skip if there is nothing to publish
        if (characteristic?.Uuid is null || value is null || _onCharacteristicChanged is null)
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        _onCharacteristicChanged.Invoke(characteristic.Uuid.ToGuid(), value);

        System.Diagnostics.Debug.WriteLine("OnCharacteristicChanged: " + stopwatch.Elapsed);
    }

    protected override bool Write(BluetoothGattCharacteristic nativeCharacteristic, byte[] data)
    {
        _writeCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var result = _bluetoothGatt?.WriteCharacteristic(nativeCharacteristic, data, (int)GattWriteType.Default);

        return result.HasValue && result.Value == 0;
    }

    protected override bool WriteNoResponse(BluetoothGattCharacteristic nativeCharacteristic, byte[] data)
    {
        var result = _bluetoothGatt?.WriteCharacteristic(nativeCharacteristic, data, (int)GattWriteType.NoResponse);
        return result.HasValue && result.Value == 0;
    }
}
