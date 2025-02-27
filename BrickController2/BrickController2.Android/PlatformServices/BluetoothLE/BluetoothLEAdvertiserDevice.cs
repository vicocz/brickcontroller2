using System.Threading.Tasks;
using Android.Bluetooth.LE;
using Android.Runtime;
using BrickController2.PlatformServices.BluetoothLE;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CA1416 // Validate platform compatibility

namespace BrickController2.Droid.PlatformServices.BluetoothLE;

internal class BluetoothLEAdvertiserDevice(BluetoothLeAdvertiser advertiser) : AdvertisingSetCallback,
    IBluetoothLEAdvertiserDevice
{
    private readonly BluetoothLeAdvertiser _advertiser = advertiser;
    private AdvertisingSet? _advertisingSet;

    public async Task<bool> StartAdvertiseAsync(AdvertisingInterval advertisingIterval, TxPowerLevel txPowerLevel, ushort manufacturerId, byte[] rawData)
    {
        AdvertisingSetParameters settings = new AdvertisingSetParameters.Builder()
            .SetLegacyMode(true)
            .SetConnectable(true)
            .SetScannable(true)
            .SetInterval(AdvertisingSetParameters.IntervalMedium)
            .SetTxPowerLevel(AdvertiseTxPower.Max)
            .Build();

        AdvertiseData data = new AdvertiseData.Builder()
            .AddManufacturerData(manufacturerId, rawData)
            .Build();

        _advertiser?.StartAdvertisingSet(
            settings,
            data,
            null,
            null,
            null,
            this);

        return await Task.FromResult(true);
    }

    public async Task<bool> StopAdvertiseAsync()
    {
        _advertiser?.StopAdvertisingSet(this);


        return await Task.FromResult(true);
    }

    public bool ChangeAdvertiseAsync(ushort manufacturerId, byte[] rawData)
    {
        if (_advertisingSet != null)
        {
            AdvertiseData data = new AdvertiseData.Builder()
                .AddManufacturerData(manufacturerId, rawData)
                .Build();

            _advertisingSet.SetAdvertisingData(data);

            return true;
        }
        else
        {
            return false;
        }
    }

    public override void OnAdvertisingSetStarted(AdvertisingSet? advertisingSet, int txPower, [GeneratedEnum] AdvertiseResult status)
    {
        base.OnAdvertisingSetStarted(advertisingSet, txPower, status);

        _advertisingSet = advertisingSet;
    }
}
