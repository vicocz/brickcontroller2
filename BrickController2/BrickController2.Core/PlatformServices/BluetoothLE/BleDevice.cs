using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Plugin.BLE.Abstractions;

using BLE = Plugin.BLE.Abstractions.Contracts;

namespace BrickController2.Core.PlatformServices.BluetoothLE;

public class BleDevice : IBluetoothLEDevice
{
    private readonly AsyncLock _lock = new();
    private readonly BLE.IAdapter _adapter;

    private BLE.IDevice? _device;
    private IReadOnlyCollection<BleGattService>? _services;

    private Action<Guid, byte[]>? _onCharacteristicChanged;
    private Action<IBluetoothLEDevice>? _onDeviceDisconnected;

    public BleDevice(BLE.IAdapter bluetoothAdapter, string address)
    {
        _adapter = bluetoothAdapter;
        Address = address;
    }

    public string Address { get; }
    public BluetoothLEDeviceState State { get; private set; } = BluetoothLEDeviceState.Disconnected;

    public async Task<IEnumerable<IGattService>?> ConnectAndDiscoverServicesAsync(
        bool autoConnect,
        Action<Guid, byte[]?> onCharacteristicChanged,
        Action<IBluetoothLEDevice> onDeviceDisconnected,
        CancellationToken token)
    {
        using var tokenRegistration = token.Register(async () =>
        {
            using (await _lock.LockAsync())
            {
                await InternalDisconnectAsync();
            }
        });
        _services = await ConnectAsync(onCharacteristicChanged, onDeviceDisconnected, token);
        return _services;
    }

    private async Task<IReadOnlyCollection<BleGattService>?> ConnectAsync(
        Action<Guid, byte[]> onCharacteristicChanged,
        Action<IBluetoothLEDevice> onDeviceDisconnected,
        CancellationToken token)
    {
        using (await _lock.LockAsync(token))
        {
            if (State != BluetoothLEDeviceState.Disconnected)
            {
                return [];
            }
            _onCharacteristicChanged = onCharacteristicChanged;
            _onDeviceDisconnected = onDeviceDisconnected;

            State = BluetoothLEDeviceState.Connecting;

            _device?.Dispose();

            // get address as GUID
            if (!Address.TryParseBluetoothAddressToGuid(out var deviceId))
            {
                deviceId = Guid.Parse(Address);
            }

            _device = await _adapter!.ConnectToKnownDeviceAsync(deviceId, new ConnectParameters(false, true), token)
                .ConfigureAwait(false);

            if (_device == null)
            {
                await InternalDisconnectAsync();
                return [];
            }
            _adapter.DeviceDisconnected += async (s, e) =>
            {
                if (_device != null && e.Device.Id == _device.Id)
                {
                    await OnDisconnectionAsync();
                }
            };
            //TODO_device.ConnectionStatusChanged += BluetoothDevice_ConnectionStatusChanged;
        }

        // enforce connection check
        return await OnConnectionAsync(token);
    }

    public async Task DisconnectAsync()
    {
        using (await _lock.LockAsync())
        {
            if (_device != null)
            {
                await _adapter!.DisconnectDeviceAsync(_device!);
            }

            await InternalDisconnectAsync();
        }
    }

    private async Task InternalDisconnectAsync()
    {
        _onDeviceDisconnected = null;
        _onCharacteristicChanged = null;

        if (_services != null)
        {
            foreach (var service in _services)
            {
                await service.DisposeAsync();
                service.Dispose();
            }
            _services = null;
        }

        if (_device != null)
        {
            //TODO_device.ConnectionStatusChanged -= BluetoothDevice_ConnectionStatusChanged;
            _device.Dispose();
            _device = null;
        }

        State = BluetoothLEDeviceState.Disconnected;
    }

    public async Task<bool> EnableNotificationAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (await _lock.LockAsync(token))
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is GattCharacteristic gattCharacteristic &&
                gattCharacteristic.CanNotify)
            {
                if (_device == null || _device.State != DeviceState.Connected)
                {
                    return false;
                }

                return await gattCharacteristic.EnableNotificationAsync(_onCharacteristicChanged!, token);
            }

            return false;
        }
    }

    public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
    {
        using (await _lock.LockAsync(token))
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is GattCharacteristic gattCharacteristic)
            {
                return await gattCharacteristic.WriteValueAsync(data);
            }
            return false;
        }
    }

    public async Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
    {
        using (await _lock.LockAsync(token))
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is GattCharacteristic gattCharacteristic)
            {
                return await gattCharacteristic.WriteValueAsync(data);
            }
            return false;
        }
    }

    public async Task<byte[]?> ReadAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (await _lock.LockAsync(token))
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is GattCharacteristic gattCharacteristic)
            {
                var result = await gattCharacteristic.ReadValueAsync(token);

                //TODO
            }
            return null;
        }
    }

    //private void BluetoothDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
    //{
    //    // check for a raise condition
    //    if (sender != _device)
    //        return;

    //    // uses lock inside OnXXX methods, execution is not awaited
    //    switch (sender.ConnectionStatus)
    //    {
    //        case BluetoothConnectionStatus.Connected:
    //            _ = OnConnection();
    //            break;

    //        case BluetoothConnectionStatus.Disconnected:
    //            _ = OnDisconnection();
    //            break;
    //    }
    //}

    private async Task<IReadOnlyCollection<BleGattService>> OnConnectionAsync(CancellationToken token = default)
    {
        using (await _lock.LockAsync(token))
        {
            if (State == BluetoothLEDeviceState.Connecting)
            {
                State = BluetoothLEDeviceState.Discovering;

                if (_device != null)
                {
                    var availableServices = await _device.GetServicesAsync(token);

                    var results = new List<BleGattService>();
                    foreach (var service in availableServices)
                    {
                        var characteristics = await service.GetCharacteristicsAsync(token);
                        // Await each item one by one before moving to the next
                        results.Add(new BleGattService(service, [.. characteristics.Select(c => new GattCharacteristic(c))]));
                    }

                    State = BluetoothLEDeviceState.Connected;
                    return [.. results];
                }
            }
            else if (State == BluetoothLEDeviceState.Connected)
            {
                // no need to react
            }
            else
            {
                await InternalDisconnectAsync();
            }
        }

        return [];
    }

    private async Task OnDisconnectionAsync()
    {
        using (await _lock.LockAsync())
        {
            switch (State)
            {
                case BluetoothLEDeviceState.Connecting:
                case BluetoothLEDeviceState.Discovering:
                    await InternalDisconnectAsync();
                    break;

                case BluetoothLEDeviceState.Connected:

                    var onDeviceDisconnected = _onDeviceDisconnected;
                    await InternalDisconnectAsync();
                    onDeviceDisconnected?.Invoke(this);
                    break;

                default:
                    break;
            }
        }
    }
}