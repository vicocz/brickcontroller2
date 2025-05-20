using System.Threading.Tasks;
using CoreBluetooth;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.iOS.PlatformServices.BluetoothLE;

internal class BluetoothLEAdvertiserDevice : CBPeripheralManagerDelegate, IBluetoothLEAdvertiserDevice
{
    private CBPeripheralManager? _peripheralManager;
    private StartAdvertisingOptions? _advData;

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
    {
        SetAdvertisingData(manufacturerId, rawData);

        return Task.CompletedTask;
    }

    public Task StopAdvertiseAsync()
    {
        StopAdvertiseInternal();

        return Task.CompletedTask;
    }

    public Task UpdateAdvertisedDataAsync(ushort manufacturerId, byte[] rawData)
    {
        SetAdvertisingData(manufacturerId, rawData);

        return Task.CompletedTask;
    }

    private void SetAdvertisingData(ushort manufacturerId, byte[] rawData)
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

        _advData = new StartAdvertisingOptions { ServicesUUID = servicesUUID };

        if (_peripheralManager?.Advertising == true)
        {
            _peripheralManager.StopAdvertising();
        }

        StartAdvertisingInternal();
    }

    private void StartAdvertisingInternal()
    {
        // Initialize peripheral manager if not already done.
        if (_peripheralManager == null)
        {
            _peripheralManager = new CBPeripheralManager();
            _peripheralManager.StateUpdated += (sender, e) =>
            {
                if (_peripheralManager.State == CBManagerState.PoweredOn && _advData != null)
                {
                    _peripheralManager.StartAdvertising(_advData);
                }
            };
        }

        if (_peripheralManager.State == CBManagerState.PoweredOn && _advData != null)
        {
            _peripheralManager.StartAdvertising(_advData);
        }
    }

    private void StopAdvertiseInternal()
    {
        _peripheralManager?.StopAdvertising();
        // Reset data so that it does not start advertising automatically if the interface goes ON.
        _advData = null;
    }
}
