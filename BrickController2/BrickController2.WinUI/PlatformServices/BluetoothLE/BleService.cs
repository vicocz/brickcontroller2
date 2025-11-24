using BrickController2.PlatformServices.BluetoothLE;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

public class BleService : IBluetoothLEService
{
    private readonly ILogger _logger;

    private bool _isScanning;

    public BleService(ILogger<BleService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> IsBluetoothLESupportedAsync()
    {
        var adapter = await BluetoothAdapter.GetDefaultAsync();
        return adapter != null && adapter.IsLowEnergySupported;
    }

    public Task<bool> IsBluetoothLEAdvertisingSupportedAsync()
        => Task.FromResult(true);
 
    public async Task<bool> IsBluetoothOnAsync()
    {
        var adapter = await BluetoothAdapter.GetDefaultAsync();
        if (adapter == null)
        {
            return false; // No Bluetooth adapter found
        }
        var radio = await adapter.GetRadioAsync();
        return radio != null && radio.State == RadioState.On;
    }

    public async Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token)
    {
        if (_isScanning || await IsBluetoothOnAsync() == false || await IsBluetoothLESupportedAsync() == false)
        {
            return false;
        }

        try
        {
            _isScanning = true;
            return await ScanAsync(scanCallback, token);
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _isScanning = false;
        }
    }

    public async Task<IBluetoothLEDevice?> GetKnownDeviceAsync(string address)
    {
        if (!await IsBluetoothLESupportedAsync())
        {
            return null;
        }

        return new BleDevice(address);
    }

    public IBluetoothLEAdvertiserDevice? CreateBluetoothLEAdvertiserDevice()
        => new BleAdvertiserDevice(_logger);

    private async Task<bool> ScanAsync(Action<ScanResult> scanCallback, CancellationToken token)
    {
        try
        {
            var leScanner = new BleScanner(scanCallback);

            leScanner.Start();

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            token.Register(() =>
            {
                leScanner.Stop();
                tcs.SetResult(true);
            });

            return await tcs.Task;
        }
        catch (Exception)
        {
            return false;
        }
    }
}