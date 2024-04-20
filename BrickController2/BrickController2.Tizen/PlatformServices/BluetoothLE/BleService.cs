using BrickController2.PlatformServices.BluetoothLE;
using Tizen.Network.Bluetooth;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

public class BleService : IBluetoothLEService
{
    [Flags]
    private enum BluetoothStatus
    {
        None = 0x00,
        ClassicSupported = 0x01,
        LowEnergySupported = 0x02,

        AllFeatures = ClassicSupported | LowEnergySupported
    }

    private bool _isScanning;

    public bool IsBluetoothLESupported => BluetoothAdapter.IsBluetoothEnabled;
    public bool IsBluetoothOn => BluetoothAdapter.IsBluetoothEnabled;

    public async Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token)
    {
        if (_isScanning || !IsBluetoothLESupported)
        {
            return false;
        }

        try
        {
            _isScanning = true;
            return await NewScanAsync(scanCallback, token);
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

    public IBluetoothLEDevice GetKnownDevice(string address)
    {
        if (!IsBluetoothLESupported)
        {
            return null;
        }

        return new BleDevice(address);
    }

    private async Task<bool> NewScanAsync(Action<ScanResult> scanCallback, CancellationToken token)
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