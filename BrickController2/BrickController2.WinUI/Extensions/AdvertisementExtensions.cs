using Windows.Devices.Bluetooth.Advertisement;

namespace BrickController2.Windows.Extensions;

public static class AdvertisementExtensions
{
    public static string GetLocalName(this BluetoothLEAdvertisementReceivedEventArgs args) => args.Advertisement.LocalName.TrimEnd();

    public static bool IsValidDeviceName(this string deviceName) => !string.IsNullOrEmpty(deviceName);

    public static bool CanCarryData(this BluetoothLEAdvertisementReceivedEventArgs args) =>
        args.AdvertisementType == BluetoothLEAdvertisementType.ScanResponse ||
        args.AdvertisementType == BluetoothLEAdvertisementType.ConnectableUndirected ||
        args.AdvertisementType == BluetoothLEAdvertisementType.NonConnectableUndirected; // some per advertisement devices which are not directly connectable
}
