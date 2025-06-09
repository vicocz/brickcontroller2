using Autofac;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.DeviceManagement.MouldKing;
using BrickController2.Droid.PlatformServices.DI;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.GameController;
using BrickController2.PlatformServices.Infrared;
using BrickController2.PlatformServices.Localization;
using BrickController2.PlatformServices.Permission;
using BrickController2.PlatformServices.SharedFileStorage;
using BrickController2.PlatformServices.Versioning;
using FluentAssertions;

namespace BrickController2.Android.Tests.PlatformServices.DI;


public class PlatformServicesModuleTests
{
    [Fact]
    public void Load_ShouldRegisterAllPlatformServices()
    {
        // Arrange
        var builder = new ContainerBuilder();

        // Act
        builder.RegisterModule<PlatformServicesModule>();
        var container = builder.Build();

        // Assert
        container.Resolve<IInfraredService>().Should().NotBeNull();
        container.Resolve<IGameControllerService>().Should().NotBeNull();
        container.Resolve<IVersionService>().Should().NotBeNull();
        container.Resolve<IBluetoothLEService>().Should().NotBeNull();
        container.Resolve<ILocalizationService>().Should().NotBeNull();
        container.Resolve<ISharedFileStorageService>().Should().NotBeNull();
        container.Resolve<IReadWriteExternalStoragePermission>().Should().NotBeNull();
        container.Resolve<IBluetoothPermission>().Should().NotBeNull();
        container.Resolve<IMKPlatformService>().Should().NotBeNull();
        container.Resolve<ICaDAPlatformService>().Should().NotBeNull();
    }

    [Fact]
    public void Load_WithDeviceManagerModule_AllCoreServicesAreResolvable()
    {
        // Arrange
        var builder = new ContainerBuilder();

        // Act
        builder.RegisterModule<PlatformServicesModule>();
        builder.RegisterModule<DeviceManagement.DI.DeviceManagementModule>();
        var container = builder.Build();

        // Assert
        container.Resolve<IBluetoothDeviceManager>().Should().NotBeNull();
        container.Resolve<IInfraredDeviceManager>().Should().NotBeNull();
        container.Resolve<IDeviceManager>().Should().NotBeNull();
        container.Resolve<IManualDeviceManager>().Should().NotBeNull();
    }
}
