using BrickController2.DeviceManagement;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement;

public class SBrickDeviceManagerTests : DeviceManagerTestBase<SBrickDeviceManager>
{
    [Fact]
    public void TryGetDevice_VengitManufacturerIdWithSBrickProductId_ReturnsSBrickDevice()
    {
        byte[] manufacturerData = [0x98, 0x01,
            0x02, 0x00, 0x00];
        var scanResult = CreateScanResult(deviceName: default, manufacturerData: manufacturerData);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.SBrick,
            ManufacturerData = manufacturerData
        });
    }

    [Fact]
    public void TryGetDevice_VengitManufacturerIdWithSBrickLightProductId_ReturnsSBrickLightDevice()
    {
        byte[] manufacturerData = [0x98, 0x01,
            0x02, 0x03, 0x00,
            0x06, 0x00, 0x01, 0x05, 0x00, 0x05, 0x19];
        var scanResult = CreateScanResult(deviceName: default, manufacturerData: manufacturerData);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.SBrickLight,
            ManufacturerData = manufacturerData
        });
    }

    [Fact]
    public void TryGetDevice_VengitManufacturerIdWithUnknownProductId_ReturnsFalse()
    {
        byte[] manufacturerData = [0x98, 0x01,
            0x06, 0x00, 0xAA, 0x00, 0x00, 0x00, 0x00];
        var scanResult = CreateScanResult(deviceName: default, manufacturerData: manufacturerData);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }

    [Fact]
    public void TryGetDevice_VengitManufacturerIdWithMissingProductId_ReturnsFalse()
    {
        byte[] manufacturerData = [0x98, 0x01,
            0x02, 0x03, 0x00];
        var scanResult = CreateScanResult(deviceName: default, manufacturerData: manufacturerData);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }

    [Fact]
    public void TryGetDevice_WrongManufacturerId_ReturnsFalse()
    {
        var scanResult = CreateScanResult("WrongManufacturerId", manufacturerData: [0x01, 0x98]);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }
}
