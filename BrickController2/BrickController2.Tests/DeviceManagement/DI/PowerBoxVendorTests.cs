using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.PowerBox;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using Xunit;
using PowerBoxVendor = BrickController2.DeviceManagement.PowerBox.PowerBox;

namespace BrickController2.Tests.DeviceManagement.DI;

public class PowerBoxVendorTests : VendorTestsBase
{
    private readonly DeviceFactory _deviceFactory;

    public PowerBoxVendorTests()
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
        builder.RegisterInstance(Mock.Of<IPowerBoxPlatformService>());

        // execute registration of vendor PowerBox
        builder.RegisterModule<PowerBoxVendor>();

        return builder;
    }

    [Theory]
    [InlineData("Device1")]
    public void RegisterDevice_PowerBox_MBattery_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.PowerBoxMBattery;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<PowerBoxMBattery>();
    }
}
