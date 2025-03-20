using BrickController2.PlatformServices.BluetoothLE;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

internal class BleAdvertiserDevice : IBluetoothLEAdvertiserDevice
{
    private BluetoothLEAdvertisementPublisher? _publisher;

    public Task StartAdvertiseAsync(AdvertisingInterval advertisingInterval,
        TxPowerLevel txPowerLevel,
        ushort manufacturerId,
        byte[] rawData)
    {
        // compose data
        var advertisement = new BluetoothLEAdvertisement()
        {
            // seems it's not allowed to setup data
            //Flags = BluetoothLEAdvertisementFlags.GeneralDiscoverableMode,
            ManufacturerData = { new BluetoothLEManufacturerData(manufacturerId, rawData.AsBuffer()) }
        };

        // start the publisher
        _publisher = new BluetoothLEAdvertisementPublisher(advertisement);
        _publisher.Start();

        return Task.CompletedTask;
    }

    public Task StopAdvertiseAsync()
    {
        _publisher?.Stop();

        return Task.CompletedTask;
    }

    public Task UpdateAdvertisedDataAsync(ushort manufacturerId, byte[] rawData)
    {
        _publisher!.Advertisement.ManufacturerData.Clear();
        _publisher!.Advertisement.ManufacturerData.Add(new BluetoothLEManufacturerData(manufacturerId, rawData.AsBuffer()));

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _publisher?.Stop();
        _publisher = null;
    }
}
