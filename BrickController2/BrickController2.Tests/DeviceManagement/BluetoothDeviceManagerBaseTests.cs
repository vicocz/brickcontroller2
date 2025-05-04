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
