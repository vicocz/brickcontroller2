using System;
using System.Collections.Generic;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Lego;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Protocols;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.Lego;

public class LegoDeviceManagerTests
{
    private readonly LegoDeviceManager _manager = new();

    [Fact]
    public void TryGetDevice_WeDoServiceUuid_WeDo2DeviceReturned()
    {
        var scanResult = new ScanResult("WeDo2", "DeviceAddress", new Dictionary<byte, byte[]>
        {
            // 128bit UUID 00001523-1212-efde-1523-785feabcd123
            { 0x06, [0x23, 0xd1, 0xbc, 0xea, 0x5f, 0x78, 0x23, 0x15, 0xde, 0xef, 0x12, 0x12, 0x23, 0x15, 0x00, 0x00] }
        });

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
        var scanResult = new ScanResult("LEGO", "DeviceAddress", new Dictionary<byte, byte[]>
        {
            { 0xff, manufacturerData }
        });

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
        var scanResult = new ScanResult("LEGO", "DeviceAddress", new Dictionary<byte, byte[]>
        {
            { 0xff, [0x97, 0x03, 0x00, 0xFF] }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.Should().NotBeNull();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }
}
