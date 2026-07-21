using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Lego;
using BrickController2.InputDeviceManagement;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.UI.Images;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using LegoVendor = BrickController2.DeviceManagement.Lego.Lego;

namespace BrickController2.Tests.DeviceManagement.DI;

public class LegoVendorTests : VendorTestsBase
{
    private readonly DeviceFactory _deviceFactory;

    public LegoVendorTests()
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
        builder.RegisterInstance(Mock.Of<IDeviceImageRegistry>()); // needed because builder.RegisterDevice<RemoteControl>().WithImage("remotecontrol_image_small.png");

        // needed because of constructor for LegoControllerService
        builder.RegisterInstance(Mock.Of<IDeviceManager>());
        builder.RegisterInstance(Mock.Of<IInputDeviceManagerService>());
        builder.RegisterInstance(Mock.Of<IInputDeviceEventServiceInternal>());
        builder.RegisterInstance(Mock.Of<ILogger<LegoControllerService>>());

        // execute registration of vendor Lego
        builder.RegisterModule<LegoVendor>();

        return builder;
    }

    [Theory]
    [InlineData("PoweredUp")]
    public void RegisterDevice_PoweredUpDevice_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.PoweredUp;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<PoweredUpDevice>();
    }

    [Theory]
    [InlineData("Boost")]
    public void RegisterDevice_BoostDevice_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.Boost;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<BoostDevice>();
    }

    [Theory]
    [InlineData("TechnicHub")]
    public void RegisterDevice_TechnicHubDevice_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.TechnicHub;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<TechnicHubDevice>();
    }

    [Theory]
    [InlineData("DuploTrainHub")]
    public void RegisterDevice_DuploTrainHubDevice_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.DuploTrainHub;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<DuploTrainHubDevice>();
    }

    [Theory]
    [InlineData("WeDo2")]
public void RegisterDevice_WeDo2Device_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.WeDo2;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<Wedo2Device>();
    }

    [Theory]
    [InlineData("TechnicMove")]
    public void RegisterDevice_TechnicMoveDevice_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.TechnicMove;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<TechnicMoveDevice>();
    }

    [Theory]
    [InlineData("RemoteControl")]
    public void RegisterDevice_RemoteControl_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.RemoteControl;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, [], []);

        device.Should().NotBeNull();
        device.Should().BeOfType<RemoteControl>();
    }
}
