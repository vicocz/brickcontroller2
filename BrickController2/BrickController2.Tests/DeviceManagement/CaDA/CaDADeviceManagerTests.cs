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
    public void CreateScanData_IosPlatform_PatchesAppIdIntoScanData()
    {
        // arrange
        _cadaPlatformService.TryGetRfPayload_ForIosPlatform();

        var scanData = _manager.CreateScanData();

        scanData.Should().BeEquivalentTo(new[]
        {
            0xC0, 0x3D, 0xCA, 0x66, 0x6D, 0x32, 0xB2, 0x9D,
            0xD2, 0x57, 0xA1, 0x5C, 0xC5, 0x05, 0xB0, 0x75,
            0xB1, 0x91, 0x48, 0x96, 0x77, 0xF8, 0x00, 0x8D,
            0x18, 0x19
        });
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
    public void TryGetDevice_CadaCarWithDifferentAppId_ReturnsFalse()
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

    [Fact]
    public void TryGetDevice_CadaCarRev2WithZeroAppId_ReturnsCaDaRaceCarRev2Device()
    {
        byte[] manufacturerData =
        [
            // manufacturerId
            0xAA,0x11,
            // CADA RaceCar
            0x11,
            // 2 bytes AppID
            0x00, 0x00,
            // other data
            0x20, 0xB9,
            // flag
            0x85,
            0x00, 0x00, 0x00, 0xA1, 0xCC, 0xB8, 0x92, 0xA0
        ];

        var scanResult = new ScanResult("RaceCar-Revision2", "AA-BB-CC-DD", new Dictionary<byte, byte[]>()
        {
            { 0xFF, manufacturerData }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = "AA-BB-CC-DD",
            DeviceName = "RaceCar-Revision2",
            DeviceType = DeviceType.CaDA_RaceCar_Rev2,
            ManufacturerData = manufacturerData
        });
    }

    [Fact]
    public void TryGetDevice_CadaCarRev2WithSomeAppId_ReturnsFalse()
    {
        byte[] manufacturerData =
        [
            // manufacturerId
            0xAA,0x11,
            // CADA RaceCar
            0x11,
            // 2 bytes AppID
            0x12, 0x34,
            // other data
            0x20, 0xB9,
            // flag
            0x86,
            0x00, 0x00, 0x00, 0xA1, 0xCC, 0xB8, 0x92, 0xA0
        ];

        var scanResult = new ScanResult("RaceCar-Revision2", "AA-BB-CC-DD", new Dictionary<byte, byte[]>()
        {
            { 0xFF, manufacturerData }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
    }
}
