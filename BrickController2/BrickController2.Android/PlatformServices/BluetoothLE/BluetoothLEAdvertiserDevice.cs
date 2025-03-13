using System;
using System.Threading.Tasks;
using Android.Bluetooth.LE;
using Android.Runtime;
using BrickController2.Droid.Extensions;
using BrickController2.PlatformServices.BluetoothLE;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CA1416 // Validate platform compatibility

namespace BrickController2.Droid.PlatformServices.BluetoothLE;

internal class BluetoothLEAdvertiserDevice(BluetoothLeAdvertiser advertiser) : AdvertisingSetCallback,
    IBluetoothLEAdvertiserDevice
{
    /// <summary>
    /// TaskCompletionSource is awaited till this Timespan expires
    /// </summary>
    private static readonly TimeSpan WaitAsyncTimeout = TimeSpan.FromMilliseconds(100);

    private readonly BluetoothLeAdvertiser _advertiser = advertiser;
    private TaskCompletionSource<bool>? _advertisingStarted;
    private TaskCompletionSource<bool>? _advertisingStopped;
    private TaskCompletionSource<bool>? _advertisingUpdated;

    private AdvertisingSet? _advertisingSet;

    public async Task StartAdvertiseAsync(AdvertisingInterval advertisingIterval, TxPowerLevel txPowerLevel, ushort manufacturerId, byte[] rawData)
    {
        AdvertisingSetParameters settings = new AdvertisingSetParameters.Builder()
            .SetLegacyMode(true)
            .SetConnectable(true)
            .SetScannable(true)
            .SetInterval(advertisingIterval.ToInterval())
            .SetTxPowerLevel(txPowerLevel.ToTxPowerLevel())
            .Build();

        AdvertiseData data = new AdvertiseData.Builder()
            .AddManufacturerData(manufacturerId, rawData)
            .Build();

        if (_advertiser != null)
        {
            TaskCompletionSource<bool> advertisingStarted = new TaskCompletionSource<bool>();
            _advertisingStarted = advertisingStarted;

            try
            {
                // https://developer.android.com/reference/android/bluetooth/le/BluetoothLeAdvertiser#startAdvertisingSet(android.bluetooth.le.AdvertisingSetParameters,%20android.bluetooth.le.AdvertiseData,%20android.bluetooth.le.AdvertiseData,%20android.bluetooth.le.PeriodicAdvertisingParameters,%20android.bluetooth.le.AdvertiseData,%20android.bluetooth.le.AdvertisingSetCallback)
                // possible exception: IllegalArgumentException
                _advertiser.StartAdvertisingSet(
                    settings,
                    data,
                    null,
                    null,
                    null,
                    this);

                // await TaskCompletionSource is set or WaitAsyncTimeout expires
                await advertisingStarted.Task.WaitAsync(WaitAsyncTimeout);
            }
            catch // don't await advertisingStarted on any exception
            {
                _advertisingStarted = null;
            }
        }
    }

    public async Task StopAdvertiseAsync()
    {
        if (_advertiser != null)
        {
            TaskCompletionSource<bool> advertisingStopped = new TaskCompletionSource<bool>();
            _advertisingStopped = advertisingStopped;

            try
            {
                _advertiser.StopAdvertisingSet(this);

                // await TaskCompletionSource is set or WaitAsyncTimeout expires
                await advertisingStopped.Task.WaitAsync(WaitAsyncTimeout);
            }
            catch // don't await advertisingStopped on any exception
            {
                _advertisingStopped = null;
            }
        }
    }

    public async Task UpdateAdvertisedDataAsync(ushort manufacturerId, byte[] rawData)
    {
        if (_advertisingSet != null)
        {
            AdvertiseData data = new AdvertiseData.Builder()
                .AddManufacturerData(manufacturerId, rawData)
                .Build();

            TaskCompletionSource<bool> advertisingUpdated = new TaskCompletionSource<bool>();
            _advertisingUpdated = advertisingUpdated;

            try
            {
                _advertisingSet.SetAdvertisingData(data);

                // await TaskCompletionSource is set or WaitAsyncTimeout expires
                await advertisingUpdated.Task.WaitAsync(WaitAsyncTimeout);
            }
            catch // don't await advertisingUpdated on any exception
            {
                _advertisingUpdated = null;
            }
        }
    }

    public override void OnAdvertisingDataSet(AdvertisingSet? advertisingSet, [GeneratedEnum] AdvertiseResult status)
    {
        base.OnAdvertisingDataSet(advertisingSet, status);

        _advertisingUpdated?.SetResult(true);
        _advertisingUpdated = null;
    }

    public override void OnAdvertisingSetStarted(AdvertisingSet? advertisingSet, int txPower, [GeneratedEnum] AdvertiseResult status)
    {
        base.OnAdvertisingSetStarted(advertisingSet, txPower, status);

        _advertisingSet = advertisingSet;

        _advertisingStarted?.SetResult(true);
        _advertisingStarted = null;
    }

    public override void OnAdvertisingSetStopped(AdvertisingSet? advertisingSet)
    {
        base.OnAdvertisingSetStopped(advertisingSet);

        _advertisingSet = null;

        _advertisingStopped?.SetResult(true);
        _advertisingStopped = null;
    }
}
