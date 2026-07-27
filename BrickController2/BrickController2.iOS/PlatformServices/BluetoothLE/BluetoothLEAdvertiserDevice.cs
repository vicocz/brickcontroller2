using System;
using System.Threading.Tasks;
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.iOS.PlatformServices.BluetoothLE;

internal class BluetoothLEAdvertiserDevice : CBPeripheralManagerDelegate, IBluetoothLEAdvertiserDevice
{
    private static readonly TimeSpan AdvertisingStartTimeout = TimeSpan.FromSeconds(5);
    private readonly object _lock = new();

    private CBPeripheralManager? _peripheralManager;
    private StartAdvertisingOptions? _advData;
    private TaskCompletionSource<NSError?>? _advertisingStartedCompletionSource;

    public BluetoothLEAdvertiserDevice()
    {
    }

    protected override void Dispose(bool disposing)
    {
        StopAdvertiseInternal();
        _peripheralManager?.Dispose();

        base.Dispose(disposing);
    }

    public Task StartAdvertiseAsync(AdvertisingInterval advertisingIterval, TxPowerLevel txPowerLevel, ushort manufacturerId, byte[] rawData)
        => SetAdvertisingDataAsync(manufacturerId, rawData, waitForAdvertisingStart: true);

    public Task StopAdvertiseAsync()
    {
        StopAdvertiseInternal();

        return Task.CompletedTask;
    }

    public Task UpdateAdvertisedDataAsync(ushort manufacturerId, byte[] rawData)
        => SetAdvertisingDataAsync(manufacturerId, rawData, waitForAdvertisingStart: false);

    public override void StateUpdated(CBPeripheralManager peripheral)
    {
        lock (_lock)
        {
            StartAdvertisingInternal();
        }
    }

    public override void AdvertisingStarted(CBPeripheralManager peripheral, NSError? error)
    {
        lock (_lock)
        {
            _advertisingStartedCompletionSource?.TrySetResult(error);
        }
    }

    private async Task SetAdvertisingDataAsync(ushort manufacturerId, byte[] rawData, bool waitForAdvertisingStart)
    {
        // JK: no check - rawDataLength has to be even!

        int entryCount = rawData.Length / 2;
        CBUUID[] servicesUUID = new CBUUID[entryCount];

        for (int index = 0; index < entryCount; index++)
        {
            int manufacturerSpecificDataIndex = index * 2;

            servicesUUID[index] = CBUUID.FromBytes([
                rawData[manufacturerSpecificDataIndex + 1],
                rawData[manufacturerSpecificDataIndex + 0]]);
        }

        TaskCompletionSource<NSError?>? advertisingStartedCompletionSource = null;

        // lock to synchronize fields _peripheralManager, _advData and _advertisingStartedCompletion
        lock (_lock)
        {
            _advData = new StartAdvertisingOptions { ServicesUUID = servicesUUID };

            if (waitForAdvertisingStart)
            {
                advertisingStartedCompletionSource = new TaskCompletionSource<NSError?>(TaskCreationOptions.RunContinuationsAsynchronously);
                _advertisingStartedCompletionSource = advertisingStartedCompletionSource;
            }

            if (_peripheralManager?.Advertising == true)
            {
                _peripheralManager.StopAdvertising();
            }

            StartAdvertisingInternal();
        }

        if (advertisingStartedCompletionSource is null)
        {
            return;
        }

        try
        {
            var completedTask = await Task.WhenAny(advertisingStartedCompletionSource.Task, Task.Delay(AdvertisingStartTimeout)).ConfigureAwait(false);
            if (completedTask != advertisingStartedCompletionSource.Task)
            {
                throw new TimeoutException($"Bluetooth LE advertising did not start within {AdvertisingStartTimeout.TotalSeconds:0} seconds. Peripheral manager state: {_peripheralManager?.State.ToString() ?? "not created"}.");
            }

            var error = await advertisingStartedCompletionSource.Task.ConfigureAwait(false);
            if (error is not null)
            {
                throw new InvalidOperationException($"Bluetooth LE advertising failed: {error.LocalizedDescription ?? error.ToString()}");
            }
        }
        finally
        {
            lock (_lock)
            {
                if (ReferenceEquals(_advertisingStartedCompletionSource, advertisingStartedCompletionSource))
                {
                    _advertisingStartedCompletionSource = null;
                }
            }
        }
    }

    /// <summary>
    /// Start advertising if the peripheral manager is powered on and advertising data is set.
    /// </summary>
    /// <remarks>
    /// ⚠️ Must be called *within a lock* to synchronize the fields _peripheralManager, _advData and _advertisingStartedCompletion.
    /// </remarks>
    private void StartAdvertisingInternal()
    {
        // Initialize peripheral manager if not already done.
        if (_peripheralManager == null)
        {
            _peripheralManager = new CBPeripheralManager(this, DispatchQueue.MainQueue);
        }

        if (_peripheralManager.State == CBManagerState.PoweredOn && _advData != null)
        {
            _peripheralManager.StartAdvertising(_advData);
            return;
        }

        if (_peripheralManager.State is CBManagerState.PoweredOff or
            CBManagerState.Unauthorized or
            CBManagerState.Unsupported)
        {
            _advertisingStartedCompletionSource?.TrySetException(new InvalidOperationException($"Bluetooth LE advertising is unavailable. Peripheral manager state: {_peripheralManager.State}."));
        }
    }

    /// <summary>
    /// Stop advertising and reset advertising data and completion source.
    /// </summary>
    private void StopAdvertiseInternal()
    {
        // lock to synchronize fields _peripheralManager, _advData and _advertisingStartedCompletion
        lock (_lock)
        {
            _peripheralManager?.StopAdvertising();
            // Reset data so that it does not start advertising automatically if the interface goes ON.
            _advData = null;
            _advertisingStartedCompletionSource?.TrySetCanceled();
            _advertisingStartedCompletionSource = null;
        }
    }
}
