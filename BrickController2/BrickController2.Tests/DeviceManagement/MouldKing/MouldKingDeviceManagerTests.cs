using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.MouldKing;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Services.AppIdentifier;
using BrickController2.UI.Services.Preferences;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.MouldKing;

public class MouldKingDeviceManagerTests
{
    private readonly MouldKingDeviceManager _manager;
    private readonly Mock<IPreferencesService> _preferencesService = new(MockBehavior.Strict);

    public MouldKingDeviceManagerTests()
    {
        _preferencesService.Setup(x => x.ContainsKey("Identifier", "App")).Returns(true);
        _preferencesService.Setup(x => x.Get("Identifier", "", "App")).Returns("YWJj");

        IAppIdentifierService appIdentifierService = new AppIdentifierService(_preferencesService.Object);
        _manager = new MouldKingDeviceManager(appIdentifierService);
    }

    [Fact]
    public void TryGetDevice_MouldKingManufacturerId_ReturnsMouldKingDiyDevice()
    {
        byte[] manufacturerData = [0x33, 0xac];

        var scanResult = new ScanResult(default, default, new Dictionary<byte, byte[]>()
        {
            { 0xFF, manufacturerData }
        });

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
        var scanResult = new ScanResult("WrongManufacturerId", default, new Dictionary<byte, byte[]>()
        {
            { 0xFF, [0x0f, 0xff] }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }

    [Fact]
    public void AppId_TwoBytesInPreferences_AllBytes()
    {
        var appId = _manager.GetAppId();
        appId.Length.Should().Be(2);
        appId.Span[0].Should().Be(0x61); // 'a' = 0x61
        appId.Span[1].Should().Be(0x62); // 'b' = 0x62
    }
}
