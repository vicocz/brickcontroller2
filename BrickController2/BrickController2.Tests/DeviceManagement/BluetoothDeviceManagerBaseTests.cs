using System;
using System.Collections.Generic;
using System.Linq;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Xunit;

namespace BrickController2.Tests.DeviceManagement;

public class BluetoothDeviceManagerBaseTests
{
    private class TestBluetoothDeviceManager : BluetoothDeviceManagerBase
    {
        protected override bool TryGetDeviceByServiceUiid(FoundDevice template, Guid serviceGuid, out FoundDevice device)
        {
            if (serviceGuid == Guid.Parse("0000180d-0000-1000-8000-00805f9b34fb")) // Example UUID
            {
                device = template with { DeviceType = DeviceType.CircuitCubes };
                return true;
            }
            device = FoundDevice.Unknown;
            return false;
        }

        protected override bool TryGetDeviceByManufacturerData(FoundDevice template, ushort manufacturerId, ReadOnlySpan<byte> manufacturerData, out FoundDevice device)
        {
            if (manufacturerId == 0x0198) // Example Manufacturer ID
            {
                device = template with { DeviceType = DeviceType.SBrick };
                return true;
            }
            device = FoundDevice.Unknown;
            return false;
        }

        protected override bool TryGetDeviceByName(FoundDevice template, ReadOnlySpan<byte> localName, out FoundDevice device)
        {
            if (localName.SequenceEqual("BuWizz"u8)) // "BuWizz"
            {
                device = template with { DeviceType = DeviceType.BuWizz };
                return true;
            }
            device = FoundDevice.Unknown;
            return false;
        }
    }

    [Fact]
    public void TryGetDevice_ReturnsFalse_WhenAdvertismentDataIsNull()
    {
        var scanResult = new ScanResult("DeviceName", "DeviceAddress", null);
        var deviceManager = new TestBluetoothDeviceManager();
        var result = deviceManager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.Should().BeEquivalentTo(FoundDevice.Unknown);
    }

    [Fact]
    public void TryGetDevice_MatchingManufacturerId_ReturnsTrueAndProperDevice()
    {
        var scanResult = new ScanResult("SBrick-ByManufacturerId-0x0198", "DeviceAddress", new Dictionary<byte, byte[]>
        {
            { 0xFF, new byte[] { 0x98, 0x01, 0x12, 0x34 } } // Manufacturer ID for SBrick
        });
        var deviceManager = new TestBluetoothDeviceManager();

        var result = deviceManager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice()
        {
            DeviceAddress = "DeviceAddress",
            DeviceName = "SBrick-ByManufacturerId-0x0198",
            DeviceType = DeviceType.SBrick,
            ManufacturerData = [0x98, 0x01, 0x12, 0x34]
        });
    }

    [Fact]
    public void TryGetDevice_UnknownManufacturerId_ReturnsFalse()
    {
        var scanResult = new ScanResult("Unknown-ByManufacturerId-0x9801", "DeviceAddress", new Dictionary<byte, byte[]>
        {
            { 0xFF, new byte[] { 0x01, 0x98 } }
        });

        var deviceManager = new TestBluetoothDeviceManager();
        var result = deviceManager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.Should().BeEquivalentTo(FoundDevice.Unknown);
    }

    [Fact]
    public void TryGetDevice_MatchingServiceUuid_ReturnsTrueAndProperDevice()
    {
        var scanResult = new ScanResult("CircuitCubes", "DeviceAddress", new Dictionary<byte, byte[]>
        { // UUID 0000180d-0000-1000-8000-00805f9b34fb
            { 0x06,[0xfb,0x34,0x9b,0x5f,0x80,0x00, 0x00, 0x80, 0x10, 0x00,0x00,0x00, 0x00, 0x00,0x18, 0x0d] }
        });
        var deviceManager = new TestBluetoothDeviceManager();

        var result = deviceManager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().NotBeNull();
        device.DeviceType.Should().Be(DeviceType.CircuitCubes);
    }

    [Fact]
    public void TryGetDevice_MatchByAdvertisedLocalName_ReturnsTrueAndProperDevice()
    {
        var scanResult = new ScanResult("BuWizzByName", "DeviceAddress", new Dictionary<byte, byte[]>
        {
            { 0x09, new byte[] { 0x42, 0x75, 0x57, 0x69, 0x7A, 0x7A } } // "BuWizz"
        });
        var deviceManager = new TestBluetoothDeviceManager();

        var result = deviceManager.TryGetDevice(scanResult, out var device);

        result.Should().BeTrue();
        device.Should().BeEquivalentTo(new FoundDevice(DeviceType.BuWizz, "BuWizzByName", "DeviceAddress"));
    }

    [Fact]
    public void TryGetDevice_NoMatchingAdvertismentData_ReturnsFalseAndUnknownDevice()
    {
        var scanResult = new ScanResult("UnknownDevice", "DeviceAddress", new Dictionary<byte, byte[]>
        {
            { 0x01, new byte[] { 0x00 } } // Unrelated data
        });
        var deviceManager = new TestBluetoothDeviceManager();

        var result = deviceManager.TryGetDevice(scanResult, out var device);

        result.Should().BeFalse();
        device.Should().BeEquivalentTo(FoundDevice.Unknown);
    }
}
