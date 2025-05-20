using BrickController2.DeviceManagement;
using FluentAssertions;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BrickController2.Tests.DeviceManagement;

public class PfxBrickDeviceManagerTests : DeviceManagerTestBase<PfxBrickDeviceManager>
{
    [Fact]
    public void TryGetDevice_ProperPfxBrickName_ReturnsPfxBrickDevice()
    {
        var scanResult = CreateScanResult(deviceName: default, new Dictionary<byte, byte[]>
        {
            {0x09, [ 0x50, 0x46, 0x78, 0x20, 0x42, 0x72, 0x69, 0x63, 0x6b, 0x20, 0x31, 0x36, 0x20, 0x4d, 0x42 ] }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.PfxBrick
        });
    }

    [Fact]
    public void TryGetDevice_WrongName_ReturnsFalse()
    {
        var scanResult = CreateScanResult(deviceName: default, new Dictionary<byte, byte[]>
        {
            {0x09, Encoding.ASCII.GetBytes("PFx Brick") }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }
}
