using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// Baseclass of Bluetooth LE Advertising devices
    /// </summary>
    internal abstract class BluetoothAdvertisingDevice : Device
    {
        /// <summary>
        /// BluetoothAdvertisingDeviceHandler
        /// </summary>
        protected readonly BluetoothAdvertisingDeviceHandler _bluetoothAdvertisingDeviceHandler;

        /// <summary>
        /// reference to bleService object
        /// </summary>
        protected readonly IBluetoothLEService _bleService;

        /// <summary>
        /// object to lock the output data
        /// </summary>
        protected readonly object _outputLock = new object();


        protected BluetoothAdvertisingDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService)
            : base(name, address, deviceRepository)
        {
            _bleService = bleService;
            _bluetoothAdvertisingDeviceHandler = GetBluetoothAdvertisingDeviceHandler();
        }

        /// <summary>
        /// manufacturerId to advertise
        /// </summary>
        protected abstract ushort ManufacturerId { get; }

        /// <summary>
        /// creates the advertising device and starts the output loop
        /// </summary>
        /// <param name="reconnect"></param>
        /// <param name="onDeviceDisconnected"></param>
        /// <param name="channelConfigurations"></param>
        /// <param name="startOutputProcessing"></param>
        /// <param name="requestDeviceInformation"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async override Task<DeviceConnectionResult> ConnectAsync(
            bool reconnect,
            Action<Device> onDeviceDisconnected,
            IEnumerable<ChannelConfiguration> channelConfigurations,
            bool startOutputProcessing,
            bool requestDeviceInformation,
            CancellationToken token)
        {
            using (await _asyncLock.LockAsync())
            {
                try
                {
                    if (!await _bluetoothAdvertisingDeviceHandler.TryConnectAsync(this))
                    {
                        return DeviceConnectionResult.Error;
                    }

                    DeviceState = DeviceState.Connecting;

                    token.ThrowIfCancellationRequested();

                    if (startOutputProcessing)
                    {
                        InitDevice();
                        
                        await _bluetoothAdvertisingDeviceHandler.StartOutputTaskAsync(this);
                    }

                    token.ThrowIfCancellationRequested();

                    DeviceState = DeviceState.Connected;
                    return DeviceConnectionResult.Ok;
                }
                catch (OperationCanceledException)
                {
                    await DisconnectAsync();

                    return DeviceConnectionResult.Canceled;
                }
                catch
                {
                    await DisconnectAsync();

                    return DeviceConnectionResult.Error;
                }
            }
        }

        /// <summary>
        /// stop output loop and dispose the advertising device
        /// </summary>
        public override async Task DisconnectAsync()
        {
            using (await _asyncLock.LockAsync())
            {
                if (DeviceState == DeviceState.Disconnected)
                {
                    return;
                }

                DeviceState = DeviceState.Disconnecting;

                await _bluetoothAdvertisingDeviceHandler.StopOutputTaskAsync(this);
                await _bluetoothAdvertisingDeviceHandler.TryDisconnectAsync(this);

                DeviceState = DeviceState.Disconnected;
            }
        }

        /// <summary>
        /// set device to initial state before output loop starts
        /// </summary>
        protected abstract void InitDevice();

        /// <summary>
        /// set device to discennected state
        /// </summary>
        protected abstract void DisconnectDevice();

        /// <summary>
        /// Get or create BluetoothAdvertisingDeviceHandler
        /// </summary>
        /// <returns>Instance of BluetoothAdvertisingDeviceHandler</returns>
        protected abstract BluetoothAdvertisingDeviceHandler GetBluetoothAdvertisingDeviceHandler();
    }
}
