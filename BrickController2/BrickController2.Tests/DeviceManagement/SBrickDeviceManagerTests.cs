using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Lego;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement;

public class SBrickDeviceManagerTests : DeviceManagerTestBase<SBrickDeviceManager>
{
    [Fact]
    public void TryGetDevice_VengitManufacturerId_ReturnsSBrickDevice()
    {
        byte[] manufacturerData = [0x98, 0x01];
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
    public void TryGetDevice_WrongManufacturerId_ReturnsFalse()
    {
        var scanResult = CreateScanResult("WrongManufacturerId", manufacturerData: [0x01, 0x98]);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }
}
