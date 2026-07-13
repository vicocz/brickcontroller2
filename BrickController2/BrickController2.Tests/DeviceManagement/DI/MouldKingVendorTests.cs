using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.MouldKing;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System;
using Xunit;
using MouldKingVendor = BrickController2.DeviceManagement.MouldKing.MouldKing;

namespace BrickController2.Tests.DeviceManagement.DI;

public class MouldKingVendorTests : VendorTestsBase
{
    private readonly DeviceFactory _deviceFactory;

    public MouldKingVendorTests()
    {
        // Act
        var container = InitializeContainer().Build();

        _deviceFactory = container.Resolve<DeviceFactory>();
    }

    protected override ContainerBuilder InitializeContainer()
    {
        var builder = base.InitializeContainer();

        // Arrange
        builder.RegisterInstance(Mock.Of<IBluetoothLEService>());
        builder.RegisterInstance(Mock.Of<IMKPlatformService>());

        // execute registration of vendor MouldKing
        builder.RegisterModule<MouldKingVendor>();

        return builder;
    }

    [Theory]
    [InlineData("Device")]        // The address is not relevant for this device
    [InlineData("IllegalDevice")] // The address is not relevant for this device
    public void RegisterDevice_MK3_8_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.MK3_8;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<MK3_8>();
    }

    [Theory]
    [InlineData("Device")]
    [InlineData("IllegalDevice")]
    public void RegisterDevice_MK4_IllegalDevice(string address)
    {
        DeviceType deviceType = DeviceType.MK4;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        Action act = () => _deviceFactory(deviceType, name, address, deviceData, []);

        act.Should().Throw<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(address));
    }

    [Theory]
    [InlineData("Device1")]
    [InlineData("Device2")]
    [InlineData("Device3")]
    public void RegisterDevice_MK4_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.MK4;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<MK4>();
    }

    [Theory]
    [InlineData("Device")]        // The address is not relevant for this device
    [InlineData("IllegalDevice")] // The address is not relevant for this device
    public void RegisterDevice_MK5_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.MK5;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<MK5>();
    }

    [Theory]
    [InlineData("Device1")]
    [InlineData("Device2")]
    [InlineData("Device3")]
    public void RegisterDevice_MK6_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.MK6;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<MK6>();
    }

    [Theory]
    [InlineData("Device")]
    [InlineData("IllegalDevice")]
    public void RegisterDevice_MK6_IllegalDevice(string address)
    {
        DeviceType deviceType = DeviceType.MK6;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        Action act = () => _deviceFactory(deviceType, name, address, deviceData, []);

        act.Should().Throw<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(address));
    }

    [Theory]
    [InlineData("MK_DIY_Address")]
    public void RegisterDevice_MKDIY_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.MK_DIY;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<MK_DIY>();
    }
}
