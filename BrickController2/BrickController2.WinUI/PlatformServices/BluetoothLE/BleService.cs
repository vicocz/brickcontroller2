using BrickController2.PlatformServices.BluetoothLE;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;

namespace BrickController2.Windows.PlatformServices.BluetoothLE;

public class BleService : IBluetoothLEService
{
    private bool _isScanning;

    public BleService()
    {
    }

    public async Task<bool> IsBluetoothLESupportedAsync()
    {
        var adapter = await BluetoothAdapter.GetDefaultAsync();
        return adapter.IsLowEnergySupported;
    }

    public Task<bool> IsBluetoothLEAdvertisingSupportedAsync()
#if DEBUG
        // JK: to allow development on windows this is enabled
        => Task.FromResult(true);
#else
        => Task.FromResult(false); // Not supported yet - has to be implemented
#endif
 
    public async Task<bool> IsBluetoothOnAsync()
    {
        var adapter = await BluetoothAdapter.GetDefaultAsync();
        var radio = await adapter.GetRadioAsync();
        return radio.State == RadioState.On;
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

    public IBluetoothLEAdvertiserDevice? CreateBluetoothLEAdvertiserDevice()
    {
        return null; // Not supported yet - has to be implemented
    }
}