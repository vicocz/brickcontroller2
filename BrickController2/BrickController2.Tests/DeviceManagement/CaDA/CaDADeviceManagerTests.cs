using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.Preferences;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.CaDA;

public class CaDADeviceManagerTests
{
    private readonly CaDADeviceManager _manager;
    private readonly Mock<IPreferencesService> _preferencesService = new(MockBehavior.Strict);
    private readonly Mock<ICaDAPlatformService> _cadaPlatformService = new(MockBehavior.Strict);

    public CaDADeviceManagerTests()
    {
        _preferencesService.Setup(x => x.ContainsKey("AppID", "CaDA")).Returns(true);
        _preferencesService.Setup(x => x.Get("AppID", "", "CaDA")).Returns("YWJj");

        _cadaPlatformService.Setup(x => x.TryGetRfPayload(It.IsAny<byte[]>(), out It.Ref<byte[]>.IsAny))
            .Callback((byte[] input, out byte[] rfPayload) =>
            {
                rfPayload = new byte[] { 0x61, 0x62, 0x63 }; // Example AppID bytes
            })
            .Returns(true);

        _manager = new CaDADeviceManager(_preferencesService.Object, _cadaPlatformService.Object);
    }

    [Fact]
    public void TryGetDevice_CadaCarWithMatchingAppId_ReturnsCaDaRaceCarDevice()
    {
        byte[] manufacturerData =
        {
            // manufacturerId
            0xf0, 0xff,
            // CADA RaceCar
            0x75, 0x49,
            // 3 bytes identifying the device
            0x01, 0x05, 0x94,
            // 3 bytes AppID
            0x61, 0x62, 0x63,
            // other data
            0x00, 0x00, 0x81, 0x82, 0x00, 0x00, 0x00, 0x00
        };

        var scanResult = new ScanResult("RaceCar", "1234", new Dictionary<byte, byte[]>()
        {
            { 0xFF, manufacturerData }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = "01-05-94",
            DeviceName = "CaDA 01-05-94",
            DeviceType = DeviceType.CaDA_RaceCar,
            ManufacturerData = manufacturerData
        });
    }

    [Fact]
    public void TryGetDevice_CadaCarWithDifferentAppId_ReturnsCaDaRaceCarDevice()
    {
        byte[] manufacturerData =
        {
            // manufacturerId
            0xf0, 0xff,
            // CADA RaceCar
            0x75, 0x40,
            // 3 bytes identifying the device
            0x01, 0x02, 0x03,
            // 3 bytes AppID
            0x63, 0x62, 0x61,
            // other data
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        var scanResult = new ScanResult("RaceCar", "1234", new Dictionary<byte, byte[]>()
        {
            { 0xFF, manufacturerData }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }
}
