using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Plugin.BLE.Abstractions;
using BLE = Plugin.BLE.Abstractions.Contracts;

namespace BrickController2.Core.PlatformServices.BluetoothLE;

public class BleDevice : IBluetoothLEDevice
{
    private readonly AsyncLock _lock = new();

    private BLE.IDevice? _device;
    private BLE.IAdapter? _adapter;
    private ICollection<BleGattService>? _services;

    private TaskCompletionSource<ICollection<BleGattService>?>? _connectCompletionSource;

    private Action<Guid, byte[]>? _onCharacteristicChanged;
    private Action<IBluetoothLEDevice>? _onDeviceDisconnected;

    public BleDevice(BLE.IAdapter bluetoothAdapter,string address)
    {
        _adapter= bluetoothAdapter;
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
                InternalDisconnect();
                _connectCompletionSource?.TrySetResult(null);
            }
        });
        _services = await ConnectAsync(onCharacteristicChanged, onDeviceDisconnected, token);
        return _services;
    }

    private async Task<ICollection<BleGattService>?> ConnectAsync(
        Action<Guid, byte[]> onCharacteristicChanged,
        Action<IBluetoothLEDevice> onDeviceDisconnected,
        CancellationToken token)
    {
        using (await _lock.LockAsync(token))
        {
            if (State != BluetoothLEDeviceState.Disconnected)
            {
                return null;
            }
            _onCharacteristicChanged = onCharacteristicChanged;
            _onDeviceDisconnected = onDeviceDisconnected;

            State = BluetoothLEDeviceState.Connecting;

            _device?.Dispose();
            var deviceId = Guid.Empty;
            _device = await _adapter!.ConnectToKnownDeviceAsync(deviceId, new ConnectParameters(true, true), token).ConfigureAwait(false);

            if (_device == null)
            {
                InternalDisconnect();
                return null;
            }
            _adapter!.DeviceDisconnected += async (s, e) =>
            {
                if (e.Device.Id == _device.Id)
                {
                    await OnDisconnection();
                }
            };
            //TODO_device.ConnectionStatusChanged += BluetoothDevice_ConnectionStatusChanged;

            _connectCompletionSource = new TaskCompletionSource<ICollection<BleGattService>?>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        // enforce connection check
        await OnConnection();

        var result = await _connectCompletionSource.Task;
        _connectCompletionSource = null;

        return result;
    }

    public async Task DisconnectAsync()
    {
        using (await _lock.LockAsync())
        {
            await _adapter!.DisconnectDeviceAsync(_device!);
            
            InternalDisconnect();
        }
    }

    private void InternalDisconnect()
    {
        _onDeviceDisconnected = null;
        _onCharacteristicChanged = null;

        if (_services != null)
        {
            foreach (var service in _services)
            {
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
                return await gattCharacteristic.EnableNotificationAsync(_onCharacteristicChanged!);
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

    private async Task OnConnection()
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connecting)
            {
                State = BluetoothLEDeviceState.Discovering;

                await DiscoverServicesAsync();
            }
            else if (State == BluetoothLEDeviceState.Connected)
            {
                // no need to react
            }
            else
            {
                InternalDisconnect();
                _connectCompletionSource?.SetResult(null);
            }
        }
    }

    private async Task OnDisconnection()
    {
        using (await _lock.LockAsync())
        {
            switch (State)
            {
                case BluetoothLEDeviceState.Connecting:
                case BluetoothLEDeviceState.Discovering:
                    InternalDisconnect();
                    _connectCompletionSource?.SetResult(null);
                    break;

                case BluetoothLEDeviceState.Connected:

                    var onDeviceDisconnected = _onDeviceDisconnected;
                    InternalDisconnect();
                    onDeviceDisconnected?.Invoke(this);
                    break;

                default:
                    break;
            }
        }
    }

    private async Task<bool> DiscoverServicesAsync(CancellationToken token=default)
    {
        // expectation is the method is already called within lock
        if (_device != null && State == BluetoothLEDeviceState.Discovering)
        {
            var availableServices = await _device.GetServicesAsync(token);

            if (availableServices is not null)
            {
                var services = availableServices.Select(s => new BleGattService(s, [])).ToArray();
                State = BluetoothLEDeviceState.Connected;
                _connectCompletionSource?.SetResult(services);
                return true;
            }
        }
        InternalDisconnect();
        _connectCompletionSource?.SetResult(null);
        return false;
    }
}