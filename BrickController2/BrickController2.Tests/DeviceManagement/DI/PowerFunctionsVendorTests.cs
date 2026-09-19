using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.PowerFunctions;
using BrickController2.PlatformServices.Infrared;
using BrickController2.UI.Images;
using FluentAssertions;
using Moq;
using Xunit;
using PowerFunctionsVendor = BrickController2.DeviceManagement.PowerFunctions.PowerFunctions;

namespace BrickController2.Tests.DeviceManagement.DI;

public class PowerFunctionsVendorTests : VendorTestsBase
{
    private readonly DeviceFactory _deviceFactory;

    public PowerFunctionsVendorTests()
    {
        // Act
        var container = InitializeContainer().Build();

        _deviceFactory = container.Resolve<DeviceFactory>();
    }

    protected override ContainerBuilder InitializeContainer()
    {
        var builder = base.InitializeContainer();

        // Arrange
        builder.RegisterInstance(Mock.Of<IInfraredService>());
        builder.RegisterInstance(Mock.Of<IDeviceImageRegistry>()); // needed because RegisterDevice<PowerFunctionsDevice>().WithImages("powerfunctions_image.png", "powerfunctions_image_small.png")

        // execute registration of vendor PowerFunctions
        builder.RegisterModule<PowerFunctionsVendor>();

        return builder;
    }


    [Theory]
    [InlineData("0", "PF Infra 1")]
    [InlineData("1", "PF Infra 2")]
    [InlineData("2", "PF Infra 3")]
    [InlineData("3", "PF Infra 4")]
    public void RegisterDevice_PowerFunctionsDevice_ReturnedDevice(string address, string name)
    {
        DeviceType deviceType = DeviceType.Infrared;

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<PowerFunctionsDevice>();
    }
}
