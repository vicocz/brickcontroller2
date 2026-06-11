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

public class VendorBuilderTests
{
    [Theory]
    [InlineData("Device")]        // The address is not relevant for this device
    [InlineData("IllegalDevice")] // The address is not relevant for this device
    public void RegisterDevice_MK3_8_ReturnedDevice(string address)
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Mock.Of<IDeviceRepository>());
        builder.RegisterInstance(Mock.Of<IBluetoothLEService>());
        builder.RegisterInstance(Mock.Of<IMKPlatformService>());

        Mock<IMouldKingDeviceManager> mouldKingDeviceManager = new();
        mouldKingDeviceManager.Setup(x => x.GetAppId()).Returns(new byte[] { 0x01, 0x02 });
        builder.RegisterInstance(mouldKingDeviceManager.Object);

        var vendorBuilder = new VendorBuilder<MouldKingVendor>(builder, new MouldKingVendor());

        // Act
        var deviceBuilder = vendorBuilder.RegisterDevice<MK3_8>();
        var container = builder.Build();

        // Assert
        deviceBuilder.Should().BeOfType<DeviceBuilder<MouldKingVendor, MK3_8>>();

        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];
        var device = container.ResolveKeyed<Device>(DeviceType.MK3_8,
            new NamedParameter(nameof(name), name),
            new NamedParameter(nameof(address), address),
            new NamedParameter(nameof(deviceData), deviceData));

        device.Should().NotBeNull();
        device.Should().BeOfType<MK3_8>();
    }

    [Theory]
    [InlineData("Device")]
    [InlineData("IllegalDevice")]
    public void RegisterDevice_MK4_IllegalDevice(string address)
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Mock.Of<IDeviceRepository>());
        builder.RegisterInstance(Mock.Of<IBluetoothLEService>());
        builder.RegisterInstance(Mock.Of<IMKPlatformService>());

        Mock<IMouldKingDeviceManager> mouldKingDeviceManager = new();
        mouldKingDeviceManager.Setup(x => x.GetAppId()).Returns(new byte[] { 0x01, 0x02 });
        builder.RegisterInstance(mouldKingDeviceManager.Object);

        var vendorBuilder = new VendorBuilder<MouldKingVendor>(builder, new MouldKingVendor());

        // Act
        var deviceBuilder = vendorBuilder.RegisterDevice<MK4>();
        var container = builder.Build();

        // Assert
        deviceBuilder.Should().BeOfType<DeviceBuilder<MouldKingVendor, MK4>>();

        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        Action act = () => container.ResolveKeyed<Device>(DeviceType.MK4,
            new NamedParameter(nameof(name), name),
            new NamedParameter(nameof(address), address),
            new NamedParameter(nameof(deviceData), deviceData));

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
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Mock.Of<IDeviceRepository>());
        builder.RegisterInstance(Mock.Of<IBluetoothLEService>());
        builder.RegisterInstance(Mock.Of<IMKPlatformService>());

        Mock<IMouldKingDeviceManager> mouldKingDeviceManager = new();
        mouldKingDeviceManager.Setup(x => x.GetAppId()).Returns(new byte[] { 0x01, 0x02 });
        builder.RegisterInstance(mouldKingDeviceManager.Object);

        var vendorBuilder = new VendorBuilder<MouldKingVendor>(builder, new MouldKingVendor());

        // Act
        var deviceBuilder = vendorBuilder.RegisterDevice<MK4>();
        var container = builder.Build();

        // Assert
        deviceBuilder.Should().BeOfType<DeviceBuilder<MouldKingVendor, MK4>>();

        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];
        var device = container.ResolveKeyed<Device>(DeviceType.MK4,
            new NamedParameter(nameof(name), name),
            new NamedParameter(nameof(address), address),
            new NamedParameter(nameof(deviceData), deviceData));

        device.Should().NotBeNull();
        device.Should().BeOfType<MK4>();
    }

    [Theory]
    [InlineData("Device")]        // The address is not relevant for this device
    [InlineData("IllegalDevice")] // The address is not relevant for this device
    public void RegisterDevice_MK5_ReturnedDevice(string address)
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Mock.Of<IDeviceRepository>());
        builder.RegisterInstance(Mock.Of<IBluetoothLEService>());
        builder.RegisterInstance(Mock.Of<IMKPlatformService>());

        Mock<IMouldKingDeviceManager> mouldKingDeviceManager = new();
        mouldKingDeviceManager.Setup(x => x.GetAppId()).Returns(new byte[] { 0x01, 0x02 });
        builder.RegisterInstance(mouldKingDeviceManager.Object);

        var vendorBuilder = new VendorBuilder<MouldKingVendor>(builder, new MouldKingVendor());

        // Act
        var deviceBuilder = vendorBuilder.RegisterDevice<MK5>();
        var container = builder.Build();

        // Assert
        deviceBuilder.Should().BeOfType<DeviceBuilder<MouldKingVendor, MK5>>();

        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];
        var device = container.ResolveKeyed<Device>(DeviceType.MK5,
            new NamedParameter(nameof(name), name),
            new NamedParameter(nameof(address), address),
            new NamedParameter(nameof(deviceData), deviceData));

        device.Should().NotBeNull();
        device.Should().BeOfType<MK5>();
    }

    [Theory]
    [InlineData("Device1")]
    [InlineData("Device2")]
    [InlineData("Device3")]
    public void RegisterDevice_MK6_ReturnedDevice(string address)
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Mock.Of<IDeviceRepository>());
        builder.RegisterInstance(Mock.Of<IBluetoothLEService>());
        builder.RegisterInstance(Mock.Of<IMKPlatformService>());

        Mock<IMouldKingDeviceManager> mouldKingDeviceManager = new();
        mouldKingDeviceManager.Setup(x => x.GetAppId()).Returns(new byte[] { 0x01, 0x02 });
        builder.RegisterInstance(mouldKingDeviceManager.Object);

        var vendorBuilder = new VendorBuilder<MouldKingVendor>(builder, new MouldKingVendor());

        // Act
        var deviceBuilder = vendorBuilder.RegisterDevice<MK6>();
        var container = builder.Build();

        // Assert
        deviceBuilder.Should().BeOfType<DeviceBuilder<MouldKingVendor, MK6>>();

        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];
        var device = container.ResolveKeyed<Device>(DeviceType.MK6,
            new NamedParameter(nameof(name), name),
            new NamedParameter(nameof(address), address),
            new NamedParameter(nameof(deviceData), deviceData));

        device.Should().NotBeNull();
        device.Should().BeOfType<MK6>();
    }

    [Theory]
    [InlineData("Device")]
    [InlineData("IllegalDevice")]
    public void RegisterDevice_MK6_IllegalDevice(string address)
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Mock.Of<IDeviceRepository>());
        builder.RegisterInstance(Mock.Of<IBluetoothLEService>());
        builder.RegisterInstance(Mock.Of<IMKPlatformService>());

        Mock<IMouldKingDeviceManager> mouldKingDeviceManager = new();
        mouldKingDeviceManager.Setup(x => x.GetAppId()).Returns(new byte[] { 0x01, 0x02 });
        builder.RegisterInstance(mouldKingDeviceManager.Object);

        var vendorBuilder = new VendorBuilder<MouldKingVendor>(builder, new MouldKingVendor());

        // Act
        var deviceBuilder = vendorBuilder.RegisterDevice<MK6>();
        var container = builder.Build();

        // Assert
        deviceBuilder.Should().BeOfType<DeviceBuilder<MouldKingVendor, MK6>>();

        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        Action act = () => container.ResolveKeyed<Device>(DeviceType.MK6,
            new NamedParameter(nameof(name), name),
            new NamedParameter(nameof(address), address),
            new NamedParameter(nameof(deviceData), deviceData));

        act.Should().Throw<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(address));
    }
}
