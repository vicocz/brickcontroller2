using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.PlatformServices.BluetoothLE;
using FluentAssertions;
using Moq;
using System;
using Xunit;
using CaDAVendor = BrickController2.DeviceManagement.CaDA.CaDA;

namespace BrickController2.Tests.DeviceManagement.DI;

public class CaDAVendorTests : VendorTestsBase
{
    private readonly DeviceFactory _deviceFactory;

    public CaDAVendorTests()
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
        builder.RegisterInstance(Mock.Of<ICaDAPlatformService>());

        // additional dependencies
        builder.RegisterInstance(Random.Shared); // MessageEncoderFactory needs Random.Shared as DI

        // execute registration of vendor CaDA
        builder.RegisterModule<CaDAVendor>();

        return builder;
    }

    [Theory]
    [InlineData("Device", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 })] // devicedata.Length of 18
    [InlineData("Device_Rev2", new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 })]    // devicedata.Length of 16
    public void RegisterDevice_CaDARaceCar_ReturnedDevice(string address, byte[] deviceData)
    {
        DeviceType deviceType = DeviceType.CaDA_RaceCar;
        string name = "TestDevice";

        var device = _deviceFactory(deviceType, name, address, deviceData, []);

        device.Should().NotBeNull();
        device.Should().BeOfType<CaDARaceCar>();
    }

    [Theory]
    [InlineData("IllegalDevice", new byte[] { 1, 2, 3, 4 })] // MessageEncoderFactory.Create needs devicedata.Length of 16 or 18
    public void RegisterDevice_CaDARaceCar_IllegalDeviceDataSize(string address, byte[] deviceData)
    {
        DeviceType deviceType = DeviceType.CaDA_RaceCar;
        string name = "TestDevice";

        Action act = () => _deviceFactory(deviceType, name, address, deviceData, []);

        act.Should().Throw<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<Autofac.Core.DependencyResolutionException>()
            .WithInnerException<InvalidOperationException>()
            .Which.Message.Should().Be("Unsupported device data format.");
    }
}
