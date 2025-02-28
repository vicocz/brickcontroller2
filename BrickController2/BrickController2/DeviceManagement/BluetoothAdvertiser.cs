using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Helpers;

namespace BrickController2.DeviceManagement
{
    /// <summary>
    /// BluetoothAdvertiser class
    /// 
    /// </summary>
    internal class BluetoothAdvertiser
    {
        /// <summary>
        /// Definition of a delegate to get 
        /// </summary>
        public delegate bool TryGetTelegramHandler(out byte[] currentData);

        /// <summary>
        /// object to lock the list handling
        /// </summary>
        protected readonly AsyncLock _asyncLock = new AsyncLock();

        /// <summary>
        /// List containing all connected devices
        /// </summary>
        private readonly List<BluetoothAdvertisingDevice> _connectedDeviceList = new List<BluetoothAdvertisingDevice>();

        /// <summary>
        /// List containing all devices in advertising state
        /// </summary>
        private readonly List<BluetoothAdvertisingDevice> _advertisingDeviceList = new List<BluetoothAdvertisingDevice>();

        /// <summary>
        /// reference to bleService object
        /// </summary>
        protected readonly IBluetoothLEService _bleService;

        /// <summary>
        /// timespan after TryGetTelegram is called from output loop to refresh data
        /// </summary>
        private readonly TimeSpan _cyclicDataRefreshTimeSpan = TimeSpan.FromSeconds(2);

        /// <summary>
        /// timespan to wait after each output loop
        /// </summary>
        private readonly TimeSpan _cyclicLoopWaitTimeSpan = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// manufacturerId to advertise
        /// </summary>
        private readonly ushort _manufacturerId;

        /// <summary>
        /// callback to get data
        /// </summary>
        private readonly TryGetTelegramHandler _tryGetTelegram;

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

        public BluetoothAdvertiser(IBluetoothLEService bleService, ushort manufacturerId, TryGetTelegramHandler tryGetTelegram)
        {
            _bleService = bleService;
            _manufacturerId = manufacturerId;
            _tryGetTelegram = tryGetTelegram;
        }

        public AdvertisingInterval AdvertisingInterval => AdvertisingInterval.Min;
        public TxPowerLevel TxPowerLevel => TxPowerLevel.Max;

        public int DataVersion { get; set; } = 0;

        public async Task<bool> TryConnectAsync(BluetoothAdvertisingDevice requestingDevice)
        {
            using (await _asyncLock.LockAsync())
            {
                // check if device is connected already
                if (_connectedDeviceList.Contains(requestingDevice))
                {
                    return true;
                }

                _connectedDeviceList.Add(requestingDevice);

                // on first connected device
                if (_connectedDeviceList.Count == 1)
                {
                    // get advertiserdevice from BLEService
                    _bleAdvertiserDevice = _bleService?.GetBluetoothLEAdvertiserDevice();
                }

                return _bleAdvertiserDevice != null;
            }
        }

        public async Task<bool> TryDisconnect(BluetoothAdvertisingDevice requestingDevice)
        {
            using (await _asyncLock.LockAsync())
            {
                // check if device is connected
                if (!_connectedDeviceList.Contains(requestingDevice))
                {
                    return false;
                }

                // remove device
                _connectedDeviceList.Remove(requestingDevice);
                _advertisingDeviceList.Remove(requestingDevice);

                // on last remove
                if (_connectedDeviceList.Count == 0 &&
                    _outputTaskTokenSource != null)
                {
                    await StopOutputTaskInternalAsync();
                }

                return true;
            }
        }

        public async Task StartOutputTaskAsync(BluetoothAdvertisingDevice requestingDevice)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!_connectedDeviceList.Contains(requestingDevice) || // requestingDevice is not connected
                  _advertisingDeviceList.Contains(requestingDevice))    // requestingDevice is added to _advertisingDeviceList already
                {
                    return; // nothing to do
                }

                // add requestingDevice to _advertisingDeviceList
                _advertisingDeviceList.Add(requestingDevice);

                // on first device added
                if (_advertisingDeviceList.Count == 1)
                {
                    StartOutputTaskInternal();
                }
            }
        }

        /// <summary>
        /// stop output loop
        /// </summary>
        public async Task StopOutputTaskAsync(BluetoothAdvertisingDevice requestingDevice)
        {
            using (await _asyncLock.LockAsync())
            {
                if (!_connectedDeviceList.Contains(requestingDevice) || // requestingDevice is not connected
                  !_advertisingDeviceList.Contains(requestingDevice))   // requestingDevice is not added to _advertisingDeviceList
                {
                    return; // nothing to do
                }

                // remove requestingDevice from _advertisingDeviceList
                _advertisingDeviceList.Remove(requestingDevice);

                // after last device removed
                if (_advertisingDeviceList.Count == 0)
                {
                    await StopOutputTaskInternalAsync();

                }
            }
        }

        /// <summary>
        /// start output loop
        /// </summary>
        private void StartOutputTaskInternal()
        {
            _outputTaskTokenSource = new CancellationTokenSource();
            CancellationToken token = _outputTaskTokenSource.Token;

            _outputTask = Task.Run(async () =>
            {
                try
                {
                    if (_bleAdvertiserDevice != null &&
                       _tryGetTelegram(out byte[] currentData))
                    {
                        _bleAdvertiserDevice.StartAdvertise(AdvertisingInterval, TxPowerLevel, _manufacturerId, currentData);

                        await ProcessOutputsAsync(token).ConfigureAwait(false);
                    }
                }
                catch (TaskCanceledException) // catch this valid exception thrown on cancellation
                {
                }
            });
        }

        /// <summary>
        /// stop output loop
        /// </summary>
        private async Task StopOutputTaskInternalAsync()
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
        private async Task ProcessOutputsAsync(CancellationToken token)
        {
            int lastChangeDataVersion = DataVersion - 1;
            int currentDataVersion;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!token.IsCancellationRequested)
            {
                currentDataVersion = DataVersion;
                bool valuesChanged = lastChangeDataVersion != currentDataVersion;

                if (valuesChanged ||
                    stopwatch.Elapsed > _cyclicDataRefreshTimeSpan)
                {
                    if (_tryGetTelegram(out byte[] currentData))
                    {
                        lastChangeDataVersion = currentDataVersion;
                        stopwatch.Restart();

                        _bleAdvertiserDevice?.UpdateAdvertisedData(_manufacturerId, currentData);
                    }
                }

                await Task.Delay(_cyclicLoopWaitTimeSpan, token).ConfigureAwait(false);
            }
        }
    }
}
