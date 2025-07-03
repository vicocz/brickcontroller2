using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

internal class BleAdvertiserDevice : IBluetoothLEAdvertiserDevice
{
    private readonly ILogger _logger;

    private BluetoothLEAdvertisementPublisher? _publisher;

    public BleAdvertiserDevice(ILogger logger)
    {
        _logger = logger;
    }

    public Task StartAdvertiseAsync(AdvertisingInterval advertisingInterval,
        TxPowerLevel txPowerLevel,
        ushort manufacturerId,
        byte[] rawData)

        => SetNewAdvertisedDataAsync(manufacturerId, rawData);

    public Task StopAdvertiseAsync()
    {
        _publisher?.Stop();
        _publisher = null;

        return Task.CompletedTask;
    }

    public Task UpdateAdvertisedDataAsync(ushort manufacturerId, byte[] rawData)
        => SetNewAdvertisedDataAsync(manufacturerId, rawData);

    public void Dispose()
    {
        _publisher?.Stop();
        _publisher = null;
    }

    private Task SetNewAdvertisedDataAsync(ushort manufacturerId, byte[] rawData)
    {
        _publisher?.Stop();

        // compose data
        var advertisement = new BluetoothLEAdvertisement()
        {
            ManufacturerData = { new BluetoothLEManufacturerData(manufacturerId, rawData.AsBuffer()) }
        };

        _publisher = new BluetoothLEAdvertisementPublisher(advertisement);
        _publisher.Start();

        _logger.LogDebug("Started BLE advertisement with Manufacturer ID: {0}, Data Length: {1}", [manufacturerId, rawData.Length]);

        return Task.CompletedTask;
    }

}
