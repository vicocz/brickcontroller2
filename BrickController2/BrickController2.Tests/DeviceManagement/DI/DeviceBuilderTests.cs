using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.MouldKing;
using BrickController2.Settings;
using FluentAssertions;
using Xunit;

using MouldKingVendor = BrickController2.DeviceManagement.MouldKing.MouldKing;

namespace BrickController2.Tests.DeviceManagement.DI;

public class DeviceBuilderTests
{
    [Fact]
    public void WithDeviceFactory_MouldKingVendor_ReturnedDeviceFactoryHasCorreectProperties()
    {
        // Arrange
        var builder = new ContainerBuilder();
        var deviceBuilder = new DeviceBuilder<MouldKingVendor, MK4>(new VendorBuilder<MouldKingVendor>(builder, new MouldKingVendor()));

        string address = "00:11:22:33:44:55";
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];
        var settings = new[]
        {
            new NamedSetting() { Name = "Setting1", Value = 42 }
        };

        // Act
        deviceBuilder.WithDeviceFactory(address, name, deviceData, settings);
        var container = builder.Build();
        var factoryData = container.Resolve<IDeviceFactoryData>();

        // Assert
        factoryData.Should().NotBeNull();
        factoryData.Name.Should().Be(name);
        factoryData.Address.Should().Be(address);
        factoryData.DeviceData.Should().BeEquivalentTo(deviceData);
        factoryData.Settings.Should().BeEquivalentTo(settings);
        factoryData.VendorName.Should().Be("Mould King");
        factoryData.DeviceTypeName.Should().Be("MK 4.0");
    }
}
