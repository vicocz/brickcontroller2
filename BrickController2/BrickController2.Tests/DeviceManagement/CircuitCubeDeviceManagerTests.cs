using BrickController2.DeviceManagement;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace BrickController2.Tests.DeviceManagement;

public class CircuitCubeDeviceManagerTests : DeviceManagerTestBase<CircuitCubeDeviceManager>
{
    [Fact]
    public void TryGetDevice_CircuitCubeServiceUuid_CircuitCubeDeviceReturned()
    {
        var scanResult = CreateScanResult(deviceName: default, serviceUuid: new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e"));

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = scanResult.DeviceAddress,
            DeviceName = scanResult.DeviceName,
            DeviceType = DeviceType.CircuitCubes,
            ManufacturerData = null
        });
    }

    [Fact]
    public void TryGetDevice_ServiceGuidDoesNotMatch_ReturnsFalse()
    {
        var scanResult = CreateScanResult("Wrong-ServiceUuid", new Dictionary<byte, byte[]>
        {
            { 0x06, new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e").ToByteArray() }
        });

        var result = _manager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.DeviceType.Should().Be(DeviceType.Unknown);
    }
}
