using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Extensions.Logging;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using System.Text;

using BLE = Plugin.BLE.Abstractions.Contracts;
namespace BrickController2.Core.PlatformServices.BluetoothLE;

public class BleService : IBluetoothLEService
{
    private readonly ILogger _logger;
    private readonly BLE.IBluetoothLE _bluetooth;

    private static readonly IReadOnlySet<AdvertisementRecordType> AdvertisementDataTypes = new HashSet<AdvertisementRecordType>(
 [
     AdvertisementRecordType.ManufacturerSpecificData,
        //AdvertisementRecordType.CompleteService128BitUuids,
        //AdvertisementRecordType.IncompleteService128BitUuids,
        AdvertisementRecordType.CompleteLocalName
 ]);

    public BleService(ILogger<BleService> logger, BLE.IBluetoothLE bluetooth)
    {
        _logger = logger;
        _bluetooth = bluetooth;
    }

    public async Task<bool> IsBluetoothLESupportedAsync() => _bluetooth.IsAvailable;

    public Task<bool> IsBluetoothLEAdvertisingSupportedAsync()
        => Task.FromResult(true);

    public async Task<bool> IsBluetoothOnAsync() => _bluetooth.IsOn;

    public async Task<bool> ScanDevicesAsync(Action<ScanResult> discoveryHandler, CancellationToken token = default)
    {
        if (_bluetooth.Adapter.IsScanning || !_bluetooth.IsOn || !_bluetooth.IsAvailable)
        {
            return false;
        }
        _bluetooth.Adapter.ScanMode = BLE.ScanMode.Balanced;
        _bluetooth.Adapter.DeviceDiscovered += ReceivedHandler;

        token.Register(async () =>
        {
            await _bluetooth.Adapter.StopScanningForDevicesAsync().ConfigureAwait(false);
            _bluetooth.Adapter.DeviceDiscovered -= ReceivedHandler;
        });

        await _bluetooth.Adapter.StartScanningForDevicesAsync(allowDuplicatesKey: false, cancellationToken:token).ConfigureAwait(false);
        return true;

        async void ReceivedHandler(object? sender, DeviceEventArgs args)
        {
            var advertisementData = args.Device.AdvertisementRecords
                .Where(s => AdvertisementDataTypes.Contains(s.Type))
                .ToDictionary(s => (byte)s.Type, s => s.Data);

            // enrich data with name manually (SBrick do not like CompleteLocalName, but Buwizz3 requires it)
            if (!advertisementData.ContainsKey((byte)AdvertisementRecordType.CompleteLocalName))
            {
                advertisementData[(byte)AdvertisementRecordType.CompleteLocalName] = Encoding.ASCII.GetBytes(args.Device.Name);
            }
            discoveryHandler(new ScanResult(args.Device.Name, args.Device.Id.ToBluetoothAddress(), advertisementData));
        }
    }

    public async Task<IBluetoothLEDevice?> GetKnownDeviceAsync(string address)
    {
        if (!await IsBluetoothLESupportedAsync())
        {
            return null;
        }

        return new BleDevice(_bluetooth.Adapter, address);
    }

    public IBluetoothLEAdvertiserDevice? CreateBluetoothLEAdvertiserDevice()
        => new BleAdvertiserDevice(_logger);
}