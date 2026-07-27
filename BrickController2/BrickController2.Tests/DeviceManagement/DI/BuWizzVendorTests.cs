using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.UI.Images;
using FluentAssertions;
using Moq;
using Xunit;
using BuWizzVendor = BrickController2.DeviceManagement.BuWizz.BuWizz;

namespace BrickController2.Tests.DeviceManagement.DI;

public class BuWizzVendorTests : VendorTestsBase
{
    private readonly DeviceFactory _deviceFactory;

    public BuWizzVendorTests()
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

        // additional dependencies
        builder.RegisterInstance(Mock.Of<IDeviceImageRegistry>()); // needed because RegisterDevice<BuWizzDevice>().WithImages("buwizz_image.png", "buwizz_image_small.png");

        // execute registration of vendor BuWizz
        builder.RegisterModule<BuWizzVendor>();

        return builder;
    }

    [Theory]
    [InlineData("BuWizzDevice")]
    public void RegisterDevice_BuWizzDevice_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.BuWizz;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<BuWizzDevice>();
    }

    [Theory]
    [InlineData("BuWizz2Device")]
    public void RegisterDevice_BuWizz2Device_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.BuWizz2;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<BuWizz2Device>();
    }

    [Theory]
    [InlineData("BuWizz3Device")]
    public void RegisterDevice_BuWizz3Device_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.BuWizz3;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<BuWizz3Device>();
    }
}
