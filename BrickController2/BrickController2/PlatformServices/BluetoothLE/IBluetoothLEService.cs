using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.PlatformServices.BluetoothLE
{
    public interface IBluetoothLEService
    {
        Task<bool> IsBluetoothLESupportedAsync();
        Task<bool> IsBluetoothLEAdvertisingSupportedAsync();
        Task<bool> IsBluetoothOnAsync();

        Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token);

        Task<IBluetoothLEDevice?> GetKnownDeviceAsync(string address);

        IBluetoothLEAdvertiserDevice? CreateBluetoothLEAdvertiserDevice();
    }
}
