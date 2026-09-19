using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Vengit;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using Xunit;
using VengitVendor = BrickController2.DeviceManagement.Vengit.Vengit;

namespace BrickController2.Tests.DeviceManagement.DI;

public class VengitVendorTests : VendorTestsBase
{
    private readonly DeviceFactory _deviceFactory;

    public VengitVendorTests()
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

        // execute registration of vendor Vengit
        builder.RegisterModule<VengitVendor>();

        return builder;
    }

    [Theory]
    [InlineData("SBrickDevice")]
    public void RegisterDevice_SBrickDevice_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.SBrick;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<SBrickDevice>();
    }

    [Theory]
    [InlineData("SBrickLight")]
    public void RegisterDevice_SBrickLight_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.SBrickLight;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<SBrickLightDevice>();
    }
}
