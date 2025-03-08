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
        /// Definition of a delegate to get telegram data
        /// </summary>
        public delegate bool TryGetTelegramHandler(bool getConnectTelegram, out byte[] telegramData);

        /// <summary>
        /// object to lock the list handling
        /// </summary>
        private readonly AsyncLock _asyncLock = new AsyncLock();

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
        /// after this timespan and all channel's values equal to zero the connect telegram is sent
        /// </summary>
        private readonly TimeSpan _reconnectTimeSpan;

        /// <summary>
        /// manufacturerId to advertise
        /// </summary>
        private readonly ushort _manufacturerId;

        /// <summary>
        /// callback to get data
        /// </summary>
        private readonly TryGetTelegramHandler _tryGetTelegram;

        /// <summary>
        /// stopwatch to measure timespan since _allChannelsZero is set to true
        /// </summary>
        private readonly Stopwatch _allZeroStopwatch = Stopwatch.StartNew();

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
        /// AutoResetEvent to signal changes of values immediately
        /// </summary>
        private AutoResetEvent? _waitForNewData;

        /// <summary>
        /// True if all channels are zero
        /// </summary>
        private bool _allChannelsZero = true;

        public BluetoothAdvertiser(IBluetoothLEService bleService, ushort manufacturerId, TryGetTelegramHandler tryGetTelegram, TimeSpan reconnectTimespan)
        {
            _bleService = bleService;
            _manufacturerId = manufacturerId;
            _tryGetTelegram = tryGetTelegram;
            _reconnectTimeSpan = reconnectTimespan;
        }

        public AdvertisingInterval AdvertisingInterval => AdvertisingInterval.Min;
        public TxPowerLevel TxPowerLevel => TxPowerLevel.Max;

        public void NotifyDataChanged(bool allChannelsZero)
        {
            // on _allChannelsZero will change to true
            if (allChannelsZero && !_allChannelsZero)
            {
                _allZeroStopwatch.Restart();
            }
            _allChannelsZero = allChannelsZero;

            // signal _waitForNewData to immediately run next loop in ProcessOutputs
            _waitForNewData?.Set();
        }

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
                // remove device
                if (!_connectedDeviceList.Remove(requestingDevice))
                {
                    // devices wasn't in list - nothing further to do
                    return false;
                }

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

            _outputTask = Task.Run(() =>
            {
                try
                {
                    if (_bleAdvertiserDevice != null &&
                       _tryGetTelegram(true, out byte[] currentData))
                    {
                        _bleAdvertiserDevice.StartAdvertise(AdvertisingInterval, TxPowerLevel, _manufacturerId, currentData);

                        _waitForNewData = new(false);

                        ProcessOutputs(token);
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

                // signal _waitForNewData to immediately run next loop in ProcessOutputs and check token.IsCancellationRequested
                _waitForNewData?.Set();

                await _outputTask;

                _outputTaskTokenSource.Dispose();
                _outputTaskTokenSource = null;

                _waitForNewData?.Dispose();
                _waitForNewData = null;

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
        private void ProcessOutputs(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_tryGetTelegram(_allChannelsZero && _allZeroStopwatch.Elapsed > _reconnectTimeSpan, out byte[] currentData))
                {
                    _bleAdvertiserDevice?.UpdateAdvertisedData(_manufacturerId, currentData);
                }

                Thread.Sleep(1); // prevent loop without sleep

                _waitForNewData?.WaitOne(_cyclicDataRefreshTimeSpan); // wait till _newData is signalled or _cyclicDataRefreshTimeSpan has passed
            }
        }
    }
}
