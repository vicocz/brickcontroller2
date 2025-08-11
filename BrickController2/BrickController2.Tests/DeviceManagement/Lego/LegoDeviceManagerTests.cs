using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Lego;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.Lego;

public class LegoDeviceManagerTests : DeviceManagerTestBase<LegoDeviceManager>
{
    [Fact]
    public void TryGetDevice_WeDoServiceUuid_WeDo2DeviceReturned()
    {
        // 128bit UUID 00001523-1212-efde-1523-785feabcd123
        var scanResult = CreateScanResult("WeDo2", advertismentData: new() { { 0x06, [0x23, 0xd1, 0xbc, 0xea, 0x5f, 0x78, 0x23, 0x15, 0xde, 0xef, 0x12, 0x12, 0x23, 0x15, 0x00, 0x00] } });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.WeDo2
        });
    }

    [Theory]
    [InlineData(0x20, DeviceType.DuploTrainHub)]
    [InlineData(0x40, DeviceType.Boost)]
    [InlineData(0x41, DeviceType.PoweredUp)]
    [InlineData(0x80, DeviceType.TechnicHub)]
    [InlineData(0x84, DeviceType.TechnicMove)]
    public void TryGetDevice_LegoManufacturerIdWithDeviceId_ReturnsProperLegoDevice(byte deviceId, DeviceType deviceType)
    {
        byte[] manufacturerData = [0x97, 0x03, 0x00, deviceId];
        var scanResult = CreateScanResult(deviceName: default, manufacturerData: manufacturerData);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = deviceType,
            ManufacturerData = manufacturerData
        });
    }

    [Fact]
    public void TryGetDevice_UnknownLegoDeviceId_ReturnsFalse()
    {
        var scanResult = CreateScanResult("UnknownLegoDevice", manufacturerData: [0x97, 0x03, 0x00, 0xFF]);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }

    [Fact]
    public void TryGetDevice_BoostWithEmptyDeviceName_ReturnsFalse()
    {
        var scanResult = CreateScanResult("", manufacturerData: [0x97, 0x03, 0x00, 0x40]);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }
}
