using BrickController2.PlatformServices.BluetoothLE;
using Tizen.Network.Bluetooth;

namespace BrickController2.Tizen.PlatformServices.BluetoothLE;

public class BleScanner
{
    private readonly Action<ScanResult> _scanCallback;

    public BleScanner(Action<ScanResult> scanCallback)
    {
        _scanCallback = scanCallback;
    }

    public void Start()
    {
        BluetoothAdapter.ScanResultChanged += BluetoothAdapter_ScanResultChanged;
        BluetoothAdapter.StartLeScan();
    }

    private void BluetoothAdapter_ScanResultChanged(object sender, AdapterLeScanResultChangedEventArgs e)
    {
        if (e?.Result != BluetoothError.None)
        {
            return;
        }

        string deviceName = e.DeviceData.GetDeviceName(BluetoothLePacketType.BluetoothLeScanResponsePacket);

        var data = e.DeviceData.GetManufacturerData(BluetoothLePacketType.BluetoothLeScanResponsePacket);

        var advertismentData =
            new Dictionary<byte, byte[]>
            {
                {0, data.Data }
            };

        _scanCallback(new ScanResult(deviceName, e.DeviceData.RemoteAddress, advertismentData));
    }

    public void Stop()
    {
        BluetoothAdapter.StopLeScan();
    }
}