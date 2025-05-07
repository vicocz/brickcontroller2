using System;
using System.Collections.Generic;
using System.Text;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.BuWizz;
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
    [InlineData(0x054e, "BuWizz", DeviceType.BuWizz2)]
    [InlineData(0x4505, "BuWizz2", DeviceType.BuWizz2)]
    [InlineData(0x054e, "BuWizz3", DeviceType.BuWizz3)]
    public void TryGetDevice_BuWizzManufacturerIdWithLocalName_ReturnsProperBuWizzDevice(ushort manufacturerId, string localName, DeviceType deviceType)
    {
        var scanResult = CreateScanResult("BuWizz", new Dictionary<byte, byte[]>
        {
            { 0xff, BitConverter.GetBytes(manufacturerId) },
            { 0x09, Encoding.ASCII.GetBytes(localName) }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = deviceType,
            ManufacturerData = BitConverter.GetBytes(manufacturerId)
        });
    }
}
