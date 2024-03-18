using BrickController2.PlatformServices.BluetoothLE;
using Tizen.Network.Bluetooth;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

public class BluetoothLEService : IBluetoothLEService
{
    private bool _isScanning = false;
    private Action<ScanResult> _scanCallback;

    public BluetoothLEService()
    {
    }

    public bool IsBluetoothLESupported => BluetoothAdapter.IsBluetoothEnabled;
    public bool IsBluetoothOn => BluetoothAdapter.IsBluetoothEnabled;

    public Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token)
    {
        if (!IsBluetoothLESupported || !IsBluetoothOn || _isScanning)
        {
            return Task.FromResult(false);
        }

        try
        {
            _scanCallback = scanCallback;
            BluetoothAdapter.ScanResultChanged += BluetoothAdapter_ScanResultChanged;
            BluetoothAdapter.StartLeScan();

            return Task.FromResult(true);
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }
        finally
        {
            BluetoothAdapter.ScanResultChanged -= BluetoothAdapter_ScanResultChanged;
            BluetoothAdapter.StopLeScan();
            _scanCallback = default;
        }
    }

    private void BluetoothAdapter_ScanResultChanged(object sender, AdapterLeScanResultChangedEventArgs e)
    {
        if (e.DeviceData is null)
        {
            return;
        }

        var deviceName = e.DeviceData.GetDeviceName(BluetoothLePacketType.BluetoothLeScanResponsePacket);
        var address = e.DeviceData.RemoteAddress;
        if (string.IsNullOrEmpty(deviceName) || string.IsNullOrEmpty(address))
        {
            return;
        }

        var advertismentData = ScanRecordProcessor.GetAdvertismentData(e.DeviceData.ScanDataInformation);
        _scanCallback(new ScanResult(deviceName, address, advertismentData));
    }

    public IBluetoothLEDevice GetKnownDevice(string address)
    {
        if (!IsBluetoothLESupported)
        {
            return null;
        }

        return new BluetoothLEDevice(address);
    }

    private async Task<bool> NewScanAsync(Action<BrickController2.PlatformServices.BluetoothLE.ScanResult> scanCallback, CancellationToken token)
    {
        try
        {
            var leScanner = new BluetoothLEScanner(scanCallback);
            var settingsBuilder = new ScanSettings.Builder()
                .SetCallbackType(ScanCallbackType.AllMatches)
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency);

            _bluetoothAdapter.BluetoothLeScanner.StartScan(null, settingsBuilder.Build(), leScanner);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(() =>
            {
                _bluetoothAdapter.BluetoothLeScanner.StopScan(leScanner);
                tcs.TrySetResult(true);
            }))
            {
                return await tcs.Task;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }
}