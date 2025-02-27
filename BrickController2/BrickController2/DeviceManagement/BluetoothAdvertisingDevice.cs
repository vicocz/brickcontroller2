using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Helpers;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// Baseclass of Bluetooth LE Advertising devices
    /// </summary>
    internal abstract class BluetoothAdvertisingDevice : Device
    {
        /// <summary>
        /// reference to bleService object
        /// </summary>
        protected readonly IBluetoothLEService _bleService;

        /// <summary>
        /// manufacturerId to advertise
        /// </summary>
        protected readonly ushort _manufacturerId;

        /// <summary>
        /// object to lock the output data
        /// </summary>
        protected readonly object _outputLock = new object();

        /// <summary>
        /// timespan after TryGetTelegram is called from output loop to refresh data
        /// </summary>
        private readonly TimeSpan _cyclicDataRefreshTimeSpan = TimeSpan.FromSeconds(2);

        /// <summary>
        /// timespan to wait after each output loop
        /// </summary>
        private readonly TimeSpan _cyclicLoopWaitTimeSpan = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// task running the cyclic output loop
        /// </summary>
        private Task? _outputTask;

        /// <summary>
        /// CancellationToken to stop the output task
        /// </summary>
        private CancellationTokenSource? _outputTaskTokenSource;

        /// <summary>
        /// BluetoothLEAdvertiserDevice created in ConnectAsync
        /// </summary>
        private IBluetoothLEAdvertiserDevice? _bleAdvertiserDevice;

        /// <summary>
        /// counter to increment if data has changed (in method "SetOutput")
        /// </summary>
        protected int _dataVersion = 0;

        protected BluetoothAdvertisingDevice(string name, string address, byte[] deviceData, IDeviceRepository deviceRepository, IBluetoothLEService bleService, ushort manufacturerId)
            : base(name, address, deviceRepository)
        {
            _bleService = bleService;
            _manufacturerId = manufacturerId;
        }

        public virtual AdvertisingInterval AdvertisingInterval => AdvertisingInterval.Min;
        public virtual TxPowerLevel TxPowerLevel => TxPowerLevel.Max;

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
                if (_bleAdvertiserDevice != null ||
                    DeviceState != DeviceState.Disconnected)
                {
                    return DeviceConnectionResult.Error;
                }

                try
                {
                    // get advertiserdevice from BLEService
                    _bleAdvertiserDevice = _bleService.GetBluetoothLEAdvertiserDevice();

                    if (_bleAdvertiserDevice == null)
                    {
                        return DeviceConnectionResult.Error;
                    }

                    DeviceState = DeviceState.Connecting;

                    token.ThrowIfCancellationRequested();

                    if (startOutputProcessing)
                    {
                        InitOutputTask();
                        await StartOutputTaskAsync();
                    }

                    token.ThrowIfCancellationRequested();

                    DeviceState = DeviceState.Connected;
                    return DeviceConnectionResult.Ok;
                }
                catch (OperationCanceledException)
                {
                    await DisconnectInternalAsync();

                    return DeviceConnectionResult.Canceled;
                }
                catch
                {
                    await DisconnectInternalAsync();

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

                await DisconnectInternalAsync();
            }
        }

        /// <summary>
        /// stop output loop and disposes the advertising device
        /// </summary>
        private async Task DisconnectInternalAsync()
        {
            if (_bleAdvertiserDevice != null)
            {
                DeviceState = DeviceState.Disconnecting;

                await StopOutputTaskAsync();

                _bleAdvertiserDevice.Dispose();
                _bleAdvertiserDevice = null;
            }

            DeviceState = DeviceState.Disconnected;
        }

        /// <summary>
        /// create a new task to start bluetooth advertising and run the output loop
        /// </summary>
        private async Task StartOutputTaskAsync()
        {
            await StopOutputTaskAsync();

            _outputTaskTokenSource = new CancellationTokenSource();
            CancellationToken token = _outputTaskTokenSource.Token;

            _outputTask = Task.Run(async () =>
            {
                byte[] currentData;
                if (_bleAdvertiserDevice != null &&
                    TryGetTelegram(out currentData))
                {
                    _bleAdvertiserDevice.StartAdvertise(AdvertisingInterval, TxPowerLevel, _manufacturerId, currentData);

                    await ProcessOutputsAsync(token).ConfigureAwait(false);
                }
            });
        }

        /// <summary>
        /// stop output loop
        /// </summary>
        private async Task StopOutputTaskAsync()
        {
            if (_outputTaskTokenSource != null &&
                _outputTask != null)
            {
                _outputTaskTokenSource.Cancel();

                await _outputTask;

                _outputTaskTokenSource.Dispose();
                _outputTaskTokenSource = null;

                _outputTask = null;
            }

            if (_bleAdvertiserDevice != null)
            {
                _bleAdvertiserDevice.StopAdvertise();
            }
        }

        /// <summary>
        /// process output loop to check for new data
        /// </summary>
        /// <param name="token">CancellationToken</param>
        protected async Task ProcessOutputsAsync(CancellationToken token)
        {
            int lastChangeDataVersion = _dataVersion - 1;
            int currentDataVersion;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!token.IsCancellationRequested)
            {
                currentDataVersion = _dataVersion;
                bool valuesChanged = lastChangeDataVersion != currentDataVersion;

                if (valuesChanged ||
                    stopwatch.Elapsed > _cyclicDataRefreshTimeSpan)
                {
                    byte[] currentData;
                    if (TryGetTelegram(out currentData))
                    {
                        lastChangeDataVersion = currentDataVersion;
                        stopwatch.Restart();

                        _bleAdvertiserDevice?.UpdateAdvertisedData(_manufacturerId, currentData);
                    }
                }

                await Task.Delay(_cyclicLoopWaitTimeSpan, token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// set device to initial state before output loop starts
        /// </summary>
        protected abstract void InitOutputTask();

        /// <summary>
        /// This method is called from the output loop in ProcessOutputsAsync if 
        /// * dataVersion has changed
        /// * cyclic after a timespan
        /// </summary>
        /// <param name="currentData">ref to byte array</param>
        /// <returns>True: success. False: no success</returns>
        public abstract bool TryGetTelegram(out byte[] currentData);
    }
}
