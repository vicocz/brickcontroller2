using System;
using System.Collections.Generic;
using System.Text;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.BuWizz;
using BrickController2.Protocols;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement.BuWizz;

public class BuWizzDeviceManagerTests : DeviceManagerTestBase<BuWizzDeviceManager>
{
    [Fact]
    public void TryGetDevice_BuWizz1ManufacturerId_BuWizz1DeviceReturned()
    {
        var scanResult = CreateScanResult("BuWizz-1-ByManufacturerId-0x484d", manufacturerData: [0x48, 0x4d, 0x98, 0x76]);

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.BuWizz,
            ManufacturerData = [0x48, 0x4d, 0x98, 0x76]
        });
    }

    [Theory]
    [InlineData(new byte[] { 0x4e, 0x05, 0x42, 0x57, 0x02, 0x01 }, DeviceType.BuWizz2)] // legacy BW2
    [InlineData(new byte[] { 0x05, 0x45, 0x42, 0x57, 0x02, 0x03 }, DeviceType.BuWizz2)] // new BW2
    [InlineData(new byte[] { 0x4e, 0x05, 0x42, 0x57, 0x03, 0x22 }, DeviceType.BuWizz3)] // BW3
    public void TryGetDevice_BuWizzManufacturerData_ReturnsProperBuWizzDevice(byte[] manufacturerData, DeviceType deviceType)
    {
        var scanResult = CreateScanResult("BuWizz", new Dictionary<byte, byte[]>
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
    public void TryGetDevice_BuWizz2ServiceUuid_ReturnsProperBuWizz2Device()
    {
        var scanResult = CreateScanResult("BuWizz2-ByUuid", new Dictionary<byte, byte[]>
        {
            { 0xff, [0x01, 0x02, 0x03] },
            { 0x07, new Guid("4e050000-74fb-4481-88b3-9919b1676e93").To128BitByteArray() }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.BuWizz2,
            ManufacturerData = [0x01, 0x02, 0x03]
        });
    }

    [Fact]
    public void TryGetDevice_BuWizz3ServiceUuid_ReturnsProperBuWizz3Device()
    {
        var scanResult = CreateScanResult("BuWizz3-ByUuid", new Dictionary<byte, byte[]>
        {
            { 0xff, BitConverter.GetBytes(0x054e) },
            { 0x09, Encoding.ASCII.GetBytes("BuWizz") },
            { 0x06, [0x93, 0x6E, 0x67, 0xB1, 0x19, 0x99, 0xB3, 0x88, 0x81, 0x44, 0xFB, 0x74, 0xD1, 0x92, 0x05, 0x50] }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.BuWizz3,
            ManufacturerData = BitConverter.GetBytes(0x054e)
        });
    }

    [Fact]
    public void TryGetDevice_BuWizz3ServiceUuidWithOtherOne_ReturnsProperBuWizz3Device()
    {
        var scanResult = CreateScanResult("BuWizz3-ByUuid", new Dictionary<byte, byte[]>
        {
            { 0x06, [0x03, 0x0E, 0x07, 0x01, 0x09, 0x09, 0x03, 0x08, 0x01, 0x04, 0x0B, 0x04, 0x01, 0x02, 0x05, 0x00,
                     0x93, 0x6E, 0x67, 0xB1, 0x19, 0x99, 0xB3, 0x88, 0x81, 0x44, 0xFB, 0x74, 0xD1, 0x92, 0x05, 0x50] }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.BuWizz3
        });
    }
}
