using BrickController2.Helpers;
using BrickController2.PlatformServices.BluetoothLE;
using Tizen.Network.Bluetooth;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

public class BleDevice : IBluetoothLEDevice
{
    private readonly AsyncLock _lock = new();

    private BluetoothGattClient _gattClient;
    private ICollection<BleGattService> _services;

    private TaskCompletionSource<ICollection<BleGattService>> _connectCompletionSource;

    private Action<Guid, byte[]> _onCharacteristicChanged;
    private Action<IBluetoothLEDevice> _onDeviceDisconnected;

    public BleDevice(string address)
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
        using (var tokenRegistration = token.Register(async () =>
        {
            using (await _lock.LockAsync())
            {
                InternalDisconnect();
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

                _gattClient?.Dispose();
                _gattClient = BluetoothGattClient.CreateClient(Address);

                _gattClient.ConnectionStateChanged += _bluetoothDevice_ConnectionStateChanged;

                await _gattClient.ConnectAsync(autoConnect);

                _connectCompletionSource = new TaskCompletionSource<ICollection<BleGattService>>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            // enforce connection check
            await OnConnection();

            var result = await _connectCompletionSource.Task;
            _connectCompletionSource = null;

            return result;
        }
    }

    public async Task DisconnectAsync()
    {
        using (await _lock.LockAsync())
        {
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
                //TODO service.Dispose();
            }
            _services = null;
        }

        if (_gattClient != null)
        {
            _gattClient.ConnectionStateChanged -= _bluetoothDevice_ConnectionStateChanged;
            _gattClient.Dispose();
            _gattClient = null;
        }

        State = BluetoothLEDeviceState.Disconnected;
    }

    public async Task<bool> EnableNotificationAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic)
            {
                //TODO
            }

            return false;
        }
    }

    public async Task<bool> DisableNotificationAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic)
            {
                //TODO
            }

            return false;
        }
    }

    public async Task<bool> WriteAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
    {
        using (await _lock.LockAsync(token))
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic)
            {
                bleGattCharacteristic.BluetoothGattCharacteristic.SetValue(data.ToString());

                var result = await _gattClient.WriteValueAsync(bleGattCharacteristic.BluetoothGattCharacteristic);
                return result;
            }
            return false;
        }
    }

    public Task<bool> WriteNoResponseAsync(IGattCharacteristic characteristic, byte[] data, CancellationToken token)
        => WriteAsync(characteristic, data, token);

    public async Task<byte[]> ReadAsync(IGattCharacteristic characteristic, CancellationToken token)
    {
        using (await _lock.LockAsync(token))
        {
            if (State == BluetoothLEDeviceState.Connected &&
                characteristic is BleGattCharacteristic bleGattCharacteristic)
            {

                var result = await _gattClient.ReadValueAsync(bleGattCharacteristic.BluetoothGattCharacteristic);

                if (result)
                {
                    var stringValue = bleGattCharacteristic.BluetoothGattCharacteristic.GetValue(0);
                    //TODO return stringValue.ToByteArray();
                    return Array.Empty<byte>();
                }
            }
            return null;
        }
    }

    private void _bluetoothDevice_ConnectionStateChanged(object sender, GattConnectionStateChangedEventArgs e)
    {
        // check for a raise condition
        if (sender != _gattClient)
            return;

        // uses lock inside OnXXX methods, execution is not awaited
        if (e.IsConnected)
        {
            _ = OnConnection();
        }
        else
        {
            _ = OnDisconnection();
        }
    }

    private async Task OnConnection()
    {
        using (await _lock.LockAsync())
        {
            if (State == BluetoothLEDeviceState.Connecting)
            {
                State = BluetoothLEDeviceState.Discovering;

                DiscoverServices();
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

    private bool DiscoverServices()
    {
        // expectation is the method is already called within lock
        if (_gattClient != null && State == BluetoothLEDeviceState.Discovering)
        {
            var services = new List<BleGattService>();
            foreach (var service in _gattClient.GetServices())
            {
                var characteristics = service.GetCharacteristics()
                    .Select(ch => new BleGattCharacteristic(ch))
                    .ToList();

                services.Add(new BleGattService(service, characteristics));
            }
            State = BluetoothLEDeviceState.Connected;
            _connectCompletionSource?.SetResult(services);
            return true;
        }
        InternalDisconnect();
        _connectCompletionSource?.SetResult(null);
        return false;
    }
}