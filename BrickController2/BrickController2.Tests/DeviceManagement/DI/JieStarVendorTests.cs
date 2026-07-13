using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.JieStar;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System;
using Xunit;
using JieStarVendor = BrickController2.DeviceManagement.JieStar.JieStar;

namespace BrickController2.Tests.DeviceManagement.DI;

public class JieStarVendorTests : VendorTestsBase
{
    private readonly DeviceFactory _deviceFactory;

    public JieStarVendorTests()
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
        builder.RegisterInstance(Mock.Of<IJieStarPlatformService>());

        // execute registration of vendor JIESTAR
        builder.RegisterModule<JieStarVendor>();

        return builder;
    }

    [Theory]
    [InlineData("Device1")]
    [InlineData("Device2")]
    [InlineData("Device3")]
    public void RegisterDevice_JieStar_SCM4_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.JieStarSCM4;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<JieStarSCM4>();
    }

    [Theory]
    [InlineData("Device")]
    [InlineData("IllegalDevice")]
    public void RegisterDevice_JieStar_SCM4_IllegalDevice(string address)
    {
        DeviceType deviceType = DeviceType.JieStarSCM4;
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
    public void RegisterDevice_JieStar_SCM8_ReturnedDevice(string address)
    {
        DeviceType deviceType = DeviceType.JieStarSCM8;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<JieStarSCM8>();
    }

    [Theory]
    [InlineData("Device")]
    [InlineData("IllegalDevice")]
    public void RegisterDevice_JieStar_SCM8_IllegalDevice(string address)
    {
        DeviceType deviceType = DeviceType.JieStarSCM8;
        string name = "TestDevice";
        byte[] deviceData = [1, 2, 3];

        Action act = () => _deviceFactory(deviceType, name, address, deviceData, []);

        act.Should().Throw<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(address));
    }
}
