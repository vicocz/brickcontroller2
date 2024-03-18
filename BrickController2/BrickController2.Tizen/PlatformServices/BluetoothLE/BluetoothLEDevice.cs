using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Tizen.Network.Bluetooth;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

public class BluetoothLEDevice : IBluetoothLEDevice
{
    //private static readonly UUID ClientCharacteristicConfigurationUUID = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

    private readonly AsyncLock _lock = new ();

    private BluetoothGattClient _bluetoothGatt = null;

    private TaskCompletionSource<IEnumerable<IGattService>> _connectCompletionSource = null;
    private TaskCompletionSource<byte[]> _readCompletionSource = null;
    private TaskCompletionSource<bool> _writeCompletionSource = null;
    private TaskCompletionSource<bool> _descriptorWriteCompletionSource = null;

    private Action<Guid, byte[]> _onCharacteristicChanged = null;
    private Action<IBluetoothLEDevice> _onDeviceDisconnected = null;

    public BluetoothLEDevice(string address)
    {
        Address = address;
    }

    public string Address { get; }
    public BluetoothLEDeviceState State { get; private set; } = BluetoothLEDeviceState.Disconnected;

    public async Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(
        bool autoConnect,
        Action<Guid, byte[]> onCharacteristicChanged,
        Action<IBluetoothLEDevice> onDeviceDisconnected,
        CancellationToken token)
    {
        using (token.Register(async () =>
        {
            using (await _lock.LockAsync())
            {
                await DisconnectAsync();
                _connectCompletionSource?.TrySetResult(null);
            }
        }))
        {
            using (await _lock.LockAsync())
            {
                if (State != BluetoothLEDeviceState.Disconnected)
                {
                    return null;
                }

                _onCharacteristicChanged = onCharacteristicChanged;
                _onDeviceDisconnected = onDeviceDisconnected;

                State = BluetoothLEDeviceState.Connecting;

                _bluetoothGatt = BluetoothGattClient.CreateClient(Address);
                _bluetoothGatt.ConnectionStateChanged += _bluetoothGatt_ConnectionStateChanged;
                await _bluetoothGatt.ConnectAsync(autoConnect);

                _connectCompletionSource = new TaskCompletionSource<IEnumerable<IGattService>>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            var result = await _connectCompletionSource.Task;

            lock (_lock)
            {
                _connectCompletionSource = null;
                return result;
            }
        }
    }

    public async Task DisconnectAsync()
    {
        using (await _lock.LockAsync())
        {
            _onDeviceDisconnected = null;
            _onCharacteristicChanged = null;

            if (_bluetoothGatt != null)
            {
                await _bluetoothGatt.DisconnectAsync();
                _bluetoothGatt.Dispose();
                _bluetoothGatt = null;
            }

            State = BluetoothLEDeviceState.Disconnected;
        }
    }

    public async Task<bool> EnableNotificationAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (token.Register(async () =>
        {
            using (await _lock.LockAsync())
            {
                _descriptorWriteCompletionSource?.TrySetResult(false);
            }
        }))

        using (await _lock.LockAsync())
        {
            if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected)
            {
                return false;
            }

            var nativeCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;
            if (!_bluetoothGatt.SetCharacteristicNotification(nativeCharacteristic, true))
            {
                return false;
            }

            var descriptor = nativeCharacteristic.GetDescriptor(ClientCharacteristicConfigurationUUID);
            if (descriptor == null)
            {
                return false;
            }

            if (!descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray()))
            {
                return false;
            }

            _descriptorWriteCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_bluetoothGatt.WriteDescriptor(descriptor))
            {
                _descriptorWriteCompletionSource = null;
                return false;
            }
        }

        var result = await _descriptorWriteCompletionSource.Task.ConfigureAwait(false);

        lock (_lock)
        {
            _descriptorWriteCompletionSource = null;
            return result;
        }
    }

    public async Task<byte[]> ReadAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (token.Register(async () =>
        {
            using (await _lock.LockAsync())
            {
                _readCompletionSource?.TrySetResult(null);
            }
        }))
        {
            using (await _lock.LockAsync())
            {
                var nativeCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;

                _readCompletionSource = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

                if (! await _bluetoothGatt.ReadValueAsync(nativeCharacteristic))
                {
                    _readCompletionSource = null;
                    return null;
                }
            }

            var result = await _readCompletionSource.Task;

            using (await _lock.LockAsync())
            {
                _readCompletionSource = null;
                return result;
            }
        }
    }

    public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
    {
        using (token.Register(async () =>
        {
            using (await _lock.LockAsync())
            {
                _writeCompletionSource?.TrySetResult(false);
            }
        }))
        {
            using (await _lock.LockAsync())
            {
                if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected)
                {
                    return false;
                }

                var nativeCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;
                nativeCharacteristic.WriteType = BluetoothGattWriteType.WriteWithResponse;

                nativeCharacteristic.SetValue(data);

                _writeCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                if (! await _bluetoothGatt.WriteValueAsync(nativeCharacteristic))
                {
                    _writeCompletionSource = null;
                    return false;
                }
            }

            var result = await _writeCompletionSource.Task.ConfigureAwait(false);

            using (await _lock.LockAsync())
            {
                _writeCompletionSource = null;
                return result;
            }
        }
    }

    public async Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
    {
        using (await _lock.LockAsync())
        {
            if (_bluetoothGatt == null || State != BluetoothLEDeviceState.Connected)
            {
                return false;
            }

            var nativeCharacteristic = ((GattCharacteristic)characteristic).BluetoothGattCharacteristic;
            nativeCharacteristic.WriteType = BluetoothGattWriteType.NoResponse;

            nativeCharacteristic.SetValue(data);

            return await _bluetoothGatt.WriteValueAsync(nativeCharacteristic);
        }
    }

    private void _bluetoothGatt_ConnectionStateChanged(object sender, GattConnectionStateChangedEventArgs e)
    {
        if (e.IsConnected)
        {
            _ = OnConnectingAsync();
        }
        else
        {
            _ = OnDisconnectingAsync();
        }
    }

    private async Task OnConnectingAsync()
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connecting)
            {
                State = BluetoothLEDeviceState.Discovering;
                Task.Run(async () =>
                {
                    using (await _lock.LockAsync())
                    {
                        if (State == BluetoothLEDeviceState.Discovering && _bluetoothGatt != null)
                        {
                            if (! await DiscoverServicesAsync())
                            {
                                await DisconnectAsync();
                                _connectCompletionSource?.TrySetResult(null);
                            }
                        }
                    }
                });
            }
            else
            {
                await DisconnectAsync();
                _connectCompletionSource?.TrySetResult(null);
            }
        }
    }

    private async Task OnDisconnectingAsync()
    {
        using (await _lock.LockAsync())
        {
            switch (State)
            {
                case BluetoothLEDeviceState.Connecting:
                case BluetoothLEDeviceState.Discovering:
                    await DisconnectAsync();
                    _connectCompletionSource?.TrySetResult(null);
                    break;

                case BluetoothLEDeviceState.Connected:
                    _writeCompletionSource?.TrySetResult(false);

                    // Copy the _onDeviceDisconnected callback to call it
                    // in case of an unexpected disconnection
                    var onDeviceDisconnected = _onDeviceDisconnected;

                    await DisconnectAsync();
                    onDeviceDisconnected?.Invoke(this);
                    break;

                default:
                    break;
            }
        }
    }

    private async Task<bool> DiscoverServicesAsync()
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Discovering)
            {
                var services = new List<GattService>();

                foreach (BluetoothGattService service in _bluetoothGatt.GetServices())
                {
                    var characteristics = new List<GattCharacteristic>();
                    {
                        foreach (var characteristic in service.GetCharacteristics())
                        {
                            characteristics.Add(new GattCharacteristic(characteristic));
                        }
                    }

                    services.Add(new GattService(service, characteristics));
                }

                State = BluetoothLEDeviceState.Connected;
                _connectCompletionSource?.TrySetResult(services);

                return true;
            }

            await DisconnectAsync();
            _connectCompletionSource?.TrySetResult(null);
            return false;
        }
    }

    public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
    {
        lock (_lock)
        {
            _readCompletionSource?.TrySetResult(characteristic.GetValue());
        }
    }

    public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
    {
        lock (_lock)
        {
            _writeCompletionSource?.TrySetResult(status == GattStatus.Success);
        }
    }

    public override void OnDescriptorWrite(BluetoothGatt gatt, BluetoothGattDescriptor descriptor, [GeneratedEnum] GattStatus status)
    {
        lock (_lock)
        {
            _descriptorWriteCompletionSource?.TrySetResult(status == GattStatus.Success);
        }
    }

    public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
    {
        lock (_lock)
        {
            var guid = characteristic.Uuid.ToGuid();
            var data = characteristic.GetValue();
            _onCharacteristicChanged?.Invoke(guid, data);
        }
    }
}