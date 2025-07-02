using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

internal class BleAdvertiserDevice : IBluetoothLEAdvertiserDevice
{
    private BluetoothLEAdvertisementPublisher? _publisher;

    public Task StartAdvertiseAsync(AdvertisingInterval advertisingInterval,
        TxPowerLevel txPowerLevel,
        ushort manufacturerId,
        byte[] rawData)
    {
        SetNewAdvertisedData(manufacturerId, rawData);

        return Task.CompletedTask;
    }

    public Task StopAdvertiseAsync()
    {
        _publisher?.Stop();
        _publisher = null;

        return Task.CompletedTask;
    }

    public Task UpdateAdvertisedDataAsync(ushort manufacturerId, byte[] rawData)
    {
        SetNewAdvertisedData(manufacturerId, rawData);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _publisher?.Stop();
        _publisher = null;
    }

    private void SetNewAdvertisedData(ushort manufacturerId, byte[] rawData)
    {
        _publisher?.Stop();

        // compose data
        var advertisement = new BluetoothLEAdvertisement()
        {
            ManufacturerData = { new BluetoothLEManufacturerData(manufacturerId, rawData.AsBuffer()) }
        };

        _publisher = new BluetoothLEAdvertisementPublisher(advertisement);
        _publisher.Start();

        // Debug.WriteLine($"Started BLE advertisement with Manufacturer ID: {manufacturerId}, Data Length: {rawData.Length}");
    }

}
