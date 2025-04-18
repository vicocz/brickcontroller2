using BrickController2.DeviceManagement;

namespace BrickController2.Helpers;

public record FoundDevice
{
    public static readonly FoundDevice Unknown = new(DeviceType.Unknown, string.Empty, string.Empty, null);

    public DeviceType DeviceType { get; init; }
    public string DeviceName { get; init; }
    public string DeviceAddress { get; init; }
    public byte[]? ManufacturerData { get; init; }

    public FoundDevice(DeviceType deviceType, string deviceName, string deviceAddress, byte[]? manufacturerData)
    {
        DeviceType = deviceType;
        DeviceName = deviceName;
        DeviceAddress = deviceAddress;
        ManufacturerData = manufacturerData;
    }
}
