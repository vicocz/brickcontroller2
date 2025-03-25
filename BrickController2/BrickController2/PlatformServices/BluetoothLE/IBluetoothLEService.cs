using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.PlatformServices.BluetoothLE
{
    public interface IBluetoothLEService
    {
        bool IsBluetoothLESupported { get; }
        bool IsBluetoothLEAdvertisingSupported { get; }
        bool IsBluetoothOn { get; }
        public string DeviceID {  get; }

        Task<bool> ScanDevicesAsync(Action<ScanResult> scanCallback, CancellationToken token);

        IBluetoothLEDevice? GetKnownDevice(string address);

        IBluetoothLEAdvertiserDevice? CreateBluetoothLEAdvertiserDevice();
    }
}
