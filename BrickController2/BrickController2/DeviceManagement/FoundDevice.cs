using BrickController2.PlatformServices.BluetoothLE;
using System;

namespace BrickController2.DeviceManagement;

public readonly record struct FoundDevice
{
    public static readonly FoundDevice Unknown = new();

    public DeviceType DeviceType { get; init; }
    public string DeviceName { get; init; }
    public string DeviceAddress { get; init; }
    public byte[]? ManufacturerData { get; init; }

    public FoundDevice()
    {
        DeviceName = string.Empty;
        DeviceAddress = string.Empty;
    }

    public FoundDevice(DeviceType deviceType, string deviceName, string deviceAddress, byte[]? manufacturerData = null)
    {
        DeviceType = deviceType;
        DeviceName = deviceName;
        DeviceAddress = deviceAddress;
        ManufacturerData = manufacturerData;
    }

    public FoundDevice(ScanResult scanResult, DeviceType deviceType, ReadOnlySpan<byte> manufacturerData)
        : this(deviceType, scanResult.DeviceName, scanResult.DeviceAddress, manufacturerData.ToArray())
    {
    }
}
