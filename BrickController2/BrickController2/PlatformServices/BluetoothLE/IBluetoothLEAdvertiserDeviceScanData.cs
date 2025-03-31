namespace BrickController2.PlatformServices.BluetoothLE;

public interface IBluetoothLEAdvertiserDeviceScanData
{
    AdvertisingInterval AdvertisingIterval { get; }
    TxPowerLevel TXPowerLevel { get; }
    ushort ManufacturerId { get; }
    
    byte[] CreateScanData();
}
