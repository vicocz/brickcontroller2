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
    /// An instance of BluetoothAdvertisingDeviceHandler is coordinating the BluetoothAdvertising 
    /// of one or multiple BluetoothAdvertisingDevices.
    /// BluetoothAdvertisingDevices have to be connected/disconnected and are requesting the 
    /// starting/stopping of the output loop.
    /// If a change of the output data is signalled BluetoothAdvertisingDeviceHandler tries to get
    /// the new data and updates the output of the data being advertised.
    /// </summary>
    internal class BluetoothAdvertisingDeviceHandler
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
        /// timespan to wait after each output loop
        /// </summary>
        private readonly TimeSpan _cyclicLoopWaitTimeSpan = TimeSpan.FromMilliseconds(100);

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
        /// A synchronization object used to ensure thread-safe operations when setting the state of all channels.
        /// </summary>
        /// <remarks>This object is intended for use in locking mechanisms to prevent race conditions
        /// during state updates.</remarks>
        private readonly object _lockAllChannelsSetState = new object();

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
        /// bitfield representing the set-state of all channels of all instances of the DeviceType handled by this instance.
        /// </summary>
        private int _allChannelsSetState = 0;

        public BluetoothAdvertisingDeviceHandler(IBluetoothLEService bleService, ushort manufacturerId, TryGetTelegramHandler tryGetTelegram, TimeSpan reconnectTimespan)
        {
            _bleService = bleService;
            _manufacturerId = manufacturerId;
            _tryGetTelegram = tryGetTelegram;
            _reconnectTimeSpan = reconnectTimespan;
        }

        public AdvertisingInterval AdvertisingInterval => AdvertisingInterval.Min;
        public TxPowerLevel TxPowerLevel => TxPowerLevel.Max;

        /// <summary>
        /// Updates the state of a specific channel by either setting or resetting its state.
        /// </summary>
        /// <remarks>This method modifies the internal state of the specified channel by either setting or
        /// resetting its corresponding bit. If <paramref name="zeroSet"/> is <see langword="true"/>, the method sets
        /// the bit for the specified channel and checks whether the state of all channels is zero and whether a change
        /// occurred. If <paramref name="zeroSet"/> is <see langword="false"/>, the method resets the bit for the
        /// specified channel and always returns <see langword="false"/>.</remarks>
        /// <param name="specificChannelNo">The zero-based index of the channel to update. Must be a valid channel number.</param>
        /// <param name="zeroSet">A value indicating whether to set (<see langword="true"/>) or reset (<see langword="false"/>) the state of
        /// the specified channel.</param>
        /// <returns><see langword="true"/> if the state of all channels is zero and the state of the specified channel was
        /// successfully changed; otherwise, <see langword="false"/>.</returns>
        public bool SetChannelState(int specificChannelNo, bool zeroSet)
        {
            lock (_lockAllChannelsSetState)
            {
                if (zeroSet)
                {
                    int allChannelsSetState = _allChannelsSetState;

                    // reset the bit for the specificChannelNo in the bitfield
                    _allChannelsSetState &= ~(1 << specificChannelNo);

                    if (_allChannelsSetState == 0 &&                    // new set state of all channels is zero
                        allChannelsSetState != _allChannelsSetState)    // and there was a change
                    {
                        // if all channels are set to zero, reset the stopwatch
                        _allZeroStopwatch.Restart();

                        return true;
                    }
                }
                else
                {
                    // set the bit for the specificChannelNo
                    _allChannelsSetState |= (1 << specificChannelNo);
                }

                return false;
            }
        }

        /// <summary>
        /// Notifies the system that data has changed and triggers any associated processes.
        /// </summary>
        /// <remarks>This method signals the system to immediately proceed with processing outputs that
        /// depend on new data. It resets internal timing mechanisms and ensures that waiting processes are
        /// notified.</remarks>
        public void NotifyDataChanged()
        {
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
                    _bleAdvertiserDevice = _bleService?.CreateBluetoothLEAdvertiserDevice();
                }

                return _bleAdvertiserDevice != null;
            }
        }

        public async Task<bool> TryDisconnectAsync(BluetoothAdvertisingDevice requestingDevice)
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
                if (_connectedDeviceList.Count == 0)
                {
                    if (_outputTaskTokenSource != null)
                    {
                        await StopOutputTaskInternalAsync();
                    }

                    _bleAdvertiserDevice?.Dispose();
                    _bleAdvertiserDevice = null;
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
                       _tryGetTelegram(true, out byte[] currentData))
                    {
                        await _bleAdvertiserDevice.StartAdvertiseAsync(AdvertisingInterval, TxPowerLevel, _manufacturerId, currentData);

                        _waitForNewData = new(false);

                        await ProcessOutputsAsync(token);
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
                await _bleAdvertiserDevice.StopAdvertiseAsync();
            }
        }

        /// <summary>
        /// process output loop to check for new data
        /// </summary>
        /// <param name="token">CancellationToken</param>
        private async Task ProcessOutputsAsync(CancellationToken token)
        {
            bool newDataSignalled = true;
            bool inConnectMode = true;
            bool inConnectModePrevious = false;

            while (!token.IsCancellationRequested)
            {
                if (_bleAdvertiserDevice != null &&
                    newDataSignalled && // different data is needed
                    _tryGetTelegram(inConnectMode, out byte[] currentData))
                {
                    await _bleAdvertiserDevice.UpdateAdvertisedDataAsync(_manufacturerId, currentData);
                    inConnectModePrevious = inConnectMode;
                }

                await Task.Delay(_cyclicLoopWaitTimeSpan, token).ConfigureAwait(false); // prevent loop without sleep

                // wait till _newData is signalled or _cyclicDataRefreshTimeSpan has passed
                newDataSignalled = _waitForNewData?.WaitOne(_cyclicDataRefreshTimeSpan) ?? false;

                // if all channels are zero and _reconnectTimeSpan has elapsed
                // then the connect telegram should be sent
                inConnectMode = _allChannelsSetState == 0 && 
                    _allZeroStopwatch.Elapsed > _reconnectTimeSpan;

                // if connectMode is requested and if previous was not a connect telegram
                if (inConnectMode && !inConnectModePrevious)
                {
                    newDataSignalled = true;    // force newDataSignalled
                }
            }
        }
    }
}
