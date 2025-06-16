using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.MouldKing;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;
using FluentAssertions;
using Moq;
using Xunit;

using MouldKingVendor = BrickController2.DeviceManagement.MouldKing.MouldKing;

namespace BrickController2.Tests.DeviceManagement.DI;

public class VendorBuilderTests
{
    [Fact]
    public void RegisterDevice_MK6_ReturnedDevice()
    {
        // Arrange
        var builder = new ContainerBuilder();
        builder.RegisterInstance(Mock.Of<IDeviceRepository>());
        builder.RegisterInstance(Mock.Of<IBluetoothLEService>());
        builder.RegisterInstance(Mock.Of<IMKPlatformService>());

        var vendorBuilder = new VendorBuilder<MouldKingVendor>(builder, new MouldKingVendor());

        // Act
        var deviceBuilder = vendorBuilder.RegisterDevice<MK6>();
        var container = builder.Build();

        // Assert
        deviceBuilder.Should().BeOfType<DeviceBuilder<MouldKingVendor, MK6>>();

        string address = "Device2";
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];
        var device = container.ResolveKeyed<Device>(DeviceType.MK6,
            new NamedParameter(nameof(name), name),
            new NamedParameter(nameof(address), address),
            new NamedParameter(nameof(deviceData), deviceData));

        device.Should().NotBeNull();
        device.Should().BeOfType<MK6>();
    }
}
