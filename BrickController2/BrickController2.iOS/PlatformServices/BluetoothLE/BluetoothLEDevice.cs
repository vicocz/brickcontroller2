using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using BrickController2.PlatformServices.BluetoothLE;
using System.Diagnostics.CodeAnalysis;

namespace BrickController2.iOS.PlatformServices.BluetoothLE
{
    public class BluetoothLEDevice : CBPeripheralDelegate, IBluetoothLEDevice
    {
        private readonly CBCentralManager _centralManager;
        private readonly CBPeripheral _peripheral;
        private readonly object _lock = new object();

        private TaskCompletionSource<IEnumerable<IGattService>>? _connectCompletionSource = null;
        private TaskCompletionSource<IEnumerable<CBCharacteristic>>? _discoverCompletionSource = null;
        private TaskCompletionSource<byte[]?>? _readCompletionSource = null;
        private TaskCompletionSource<bool>? _writeCompletionSource = null;

        private Action<Guid, byte[]?>? _onCharacteristicChanged = null;
        private Action<IBluetoothLEDevice>? _onDeviceDisconnected = null;

        public BluetoothLEDevice(CBCentralManager centralManager, CBPeripheral peripheral)
        {
            _peripheral = peripheral;
            _peripheral.Delegate = this;
            _centralManager = centralManager;
        }

        public string Address => _peripheral.Identifier!.ToString();
        public BluetoothLEDeviceState State { get; private set; } = BluetoothLEDeviceState.Disconnected;

        public async Task<IEnumerable<IGattService>> ConnectAndDiscoverServicesAsync(
            bool autoConnect,
            Action<Guid, byte[]?> onCharacteristicChanged,
            Action<IBluetoothLEDevice> onDeviceDisconnected,
            CancellationToken token)
        {
            using (token.Register(() =>
            {
                lock (_lock)
                {
                    Disconnect();
                    _connectCompletionSource?.TrySetResult([]);
                }
            }))
            {
                lock (_lock)
                {
                    if (State != BluetoothLEDeviceState.Disconnected)
                    {
                        return [];
                    }

                    _onCharacteristicChanged = onCharacteristicChanged;
                    _onDeviceDisconnected = onDeviceDisconnected;

                    State = BluetoothLEDeviceState.Connecting;
                    _centralManager.ConnectPeripheral(_peripheral, new PeripheralConnectionOptions { NotifyOnConnection = true, NotifyOnDisconnection = true });

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

        internal void Disconnect()
        {
            _onDeviceDisconnected = null;
            _onCharacteristicChanged = null;

            _centralManager.CancelPeripheralConnection(_peripheral);
            State = BluetoothLEDeviceState.Disconnected;
        }

        public Task DisconnectAsync()
        {
            lock (_lock)
            {
                Disconnect();
            }

            return Task.CompletedTask;
        }

        public Task<bool> EnableNotificationAsync(Guid characteristic, CancellationToken token)
        {
            lock(_lock)
            {
                if (TryGetCharacteristic(characteristic, out var nativeCharacteristic))
                {
                    _peripheral.SetNotifyValue(true, nativeCharacteristic);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
        }

        public async Task<byte[]?> ReadAsync(Guid characteristic, CancellationToken token)
        {
            using (token.Register(() =>
            {
                lock (_lock)
                {
                    _readCompletionSource?.TrySetResult(null);
                }
            }))
            {
                lock (_lock)
                {
                    if (State != BluetoothLEDeviceState.Connected || !TryGetCharacteristic(characteristic, out var nativeCharacteristic))
                    {
                        return null;
                    }
                    _readCompletionSource = new TaskCompletionSource<byte[]?>(TaskCreationOptions.RunContinuationsAsynchronously);

                    _peripheral.ReadValue(nativeCharacteristic);
                }

                var result = await _readCompletionSource.Task;

                lock (_lock)
                {
                    _readCompletionSource = null;
                    return result;
                }
            }
        }

        public async Task<bool> WriteAsync(Guid characteristic, byte[] data, CancellationToken token)
        {
            using (token.Register(() =>
            {
                lock (_lock)
                {
                    _writeCompletionSource?.TrySetResult(false);
                }
            }))
            {
                lock (_lock)
                {
                    if (State != BluetoothLEDeviceState.Connected || !TryGetCharacteristic(characteristic, out var nativeCharacteristic))
                    {
                        return false;
                    }
                    var nativeData = NSData.FromArray(data);

                    _writeCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                    _peripheral.WriteValue(nativeData, nativeCharacteristic, CBCharacteristicWriteType.WithResponse);
                }

                var result = await _writeCompletionSource.Task;

                lock (_lock)
                {
                    _writeCompletionSource = null;
                    return result;
                }
            }
        }

        public Task<bool> WriteNoResponseAsync(Guid characteristic, byte[] data, CancellationToken token)
        {
            lock (_lock)
            {
                if (State != BluetoothLEDeviceState.Connected || !TryGetCharacteristic(characteristic, out var nativeCharacteristic))
                {
                    return Task.FromResult(false);
                }
                var nativeData = NSData.FromArray(data);

                _peripheral.WriteValue(nativeData, nativeCharacteristic, CBCharacteristicWriteType.WithoutResponse);
                return Task.FromResult(true);
            }
        }

        public override async void DiscoveredService(CBPeripheral peripheral, NSError? error)
        {
            try
            {
                if (error is null)
                {
                    var services = new List<GattService>();
                    if (_peripheral?.Services is not null)
                    {
                        foreach (var service in _peripheral.Services)
                        {
                            _discoverCompletionSource = new TaskCompletionSource<IEnumerable<CBCharacteristic>>(TaskCreationOptions.RunContinuationsAsynchronously);

                            _peripheral.DiscoverCharacteristics(service);

                            var result = await _discoverCompletionSource.Task;
                            _discoverCompletionSource = null;

                            if (result is not null)
                            {
                                services.Add(new GattService(service, result));
                            }
                            else
                            {
                                lock (_lock)
                                {
                                    Disconnect();
                                    _connectCompletionSource?.TrySetResult([]);
                                }
                                return;
                            }
                        }
                    }

                    lock (_lock)
                    {
                        State = BluetoothLEDeviceState.Connected;
                        _connectCompletionSource?.TrySetResult(services);
                        return;
                    }
                }
                else
                {
                    lock(_lock)
                    {
                        Disconnect();
                        _connectCompletionSource?.TrySetResult([]);
                    }
                }
            }
            catch (Exception)
            {
                lock (_lock)
                {
                    Disconnect();
                    _connectCompletionSource?.TrySetResult([]);
                }
            }
        }

        public override void DiscoveredCharacteristics(CBPeripheral peripheral, CBService service, NSError? error)
        {
            lock (_lock)
            {
                try
                {
                    if (error == null)
                    {
                        var characteristics = new List<CBCharacteristic>();
                        if (service.Characteristics is not null)
                        {
                            characteristics.AddRange(service.Characteristics);
                        }

                        _discoverCompletionSource?.TrySetResult(characteristics);
                    }
                    else
                    {
                        _discoverCompletionSource?.TrySetResult([]);
                    }
                }
                catch (Exception)
                {
                    _discoverCompletionSource?.TrySetResult([]);
                }
            }
        }

        public override void UpdatedCharacterteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError? error)
        {
            lock(_lock)
            {
                var data = error is null ? characteristic.Value?.ToArray() : null;

                if (_readCompletionSource is not null)
                {
                    _readCompletionSource.TrySetResult(data);
                }
                else
                {
                    if (error is null)
                    {
                        var guid = characteristic.UUID.ToGuid();
                        _onCharacteristicChanged?.Invoke(guid, data);
                    }
                }
            }
        }

        public override void WroteCharacteristicValue(CBPeripheral peripheral, CBCharacteristic characteristic, NSError? error)
        {
            lock(_lock)
            {
                _writeCompletionSource?.TrySetResult(error == null);
            }
        }

        internal void OnDeviceConnected()
        {
            lock (_lock)
            {
                if (State == BluetoothLEDeviceState.Connecting)
                {
                    State = BluetoothLEDeviceState.Discovering;
                    Task.Run(() =>
                    {
                        Thread.Sleep(750);

                        lock (_lock)
                        {
                            if (State == BluetoothLEDeviceState.Discovering)
                            {
                                _peripheral.DiscoverServices();
                            }
                        }
                    });
                }
            }
        }

        internal void OnDeviceDisconnected()
        {
            lock (_lock)
            {
                switch (State)
                {
                    case BluetoothLEDeviceState.Connecting:
                    case BluetoothLEDeviceState.Discovering:
                        Disconnect();
                        _connectCompletionSource?.TrySetResult([]);
                        break;

                    case BluetoothLEDeviceState.Connected:
                        _writeCompletionSource?.TrySetResult(false);

                        // Copy the _onDeviceDisconnected callback to call it
                        // in case of an unexpected disconnection
                        var onDeviceDisconnected = _onDeviceDisconnected;

                        Disconnect();
                        onDeviceDisconnected?.Invoke(this);
                        break;

                    default:
                        break;
                }
            }
        }

        private bool TryGetCharacteristic(Guid characteristic, [NotNullWhen(true)] out CBCharacteristic? nativeCharacteristic)
        {
            nativeCharacteristic = null;
            if (_peripheral?.Services is not null)
            {
                foreach (var service in _peripheral.Services)
                {
                    if (service.Characteristics is not null)
                    {
                        foreach (var charac in service.Characteristics)
                        {
                            if (charac.UUID.ToGuid() == characteristic)
                            {
                                nativeCharacteristic = charac;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}