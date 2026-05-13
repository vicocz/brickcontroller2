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
            // Seed
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
            DeviceType = DeviceType.CaDA_RaceCar,
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
            // Seed
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

    [Fact]
    public void AppId_ThreeBytesInPreferences_AllBytes()
    {
        var appId = _manager.GetAppId();
        appId.Length.Should().Be(3);
        appId.Span[0].Should().Be(0x61); // 'a' = 0x61
        appId.Span[1].Should().Be(0x62); // 'b' = 0x62
        appId.Span[2].Should().Be(0x63); // 'c' = 0x63
    }

    [Fact]
    public void CreateScanData_IosPlatform_PatchesAppIdIntoScanData()
    {
        // arrange
        _cadaPlatformService.Setup(x => x.TryGetRfPayload(It.IsAny<byte[]>(), out It.Ref<byte[]>.IsAny))
            .Callback((byte[] input, out byte[] rfPayload) =>
            {
                rfPayload = input;
            })
            .Returns(true);

        var scanData = _manager.CreateScanData();
        // Assert
        scanData.Should().Equal(
        [
              0x75, //  [0] const 0x75 (117)
              0x10, //  [1] 0x17 (23) STATUS_UNPAIRING - else - 0x10 (16)
              0x00, //  [2] DeviceAddress
              0x00, //  [3] DeviceAddress
              0x00, //  [4] DeviceAddress
              0x61, //  [5] AppID
              0x62, //  [6] AppID
              0x63, //  [7] AppID
              0x00, //  [8] 
              0x00, //  [9] 
              0x80, // [10] min 128
              0x80, // [11] min 128
              0x00, // [12] 
              0x00, // [13] 
              0x00, // [14] 
              0x00, // [15] 
        ]);
    }
}
