using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.MouldKing;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.MouldKing;

public class MouldKingDeviceManagerTests : DeviceManagerTestBase<MouldKingDeviceManager>
{
    [Fact]
    public void TryGetDevice_MouldKingManufacturerId_ReturnsMouldKingDiyDevice()
    {
        byte[] manufacturerData = [0x33, 0xac];
        var scanResult = CreateScanResult(deviceName: default, manufacturerData: manufacturerData);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.MK_DIY,
            ManufacturerData = manufacturerData
        });
    }

    [Fact]
    public void TryGetDevice_WrongManufacturerId_ReturnsFalse()
    {
        var scanResult = CreateScanResult("WrongManufacturerId", manufacturerData: [0x0f, 0xff]);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }
}
