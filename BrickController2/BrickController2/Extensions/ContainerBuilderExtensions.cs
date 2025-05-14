using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using BrickController2.DeviceManagement;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;

namespace BrickController2.Extensions;

public static class ContainerBuilderExtensions
{
    public static void RegisterDeviceFactory(this ContainerBuilder builder, DeviceType deviceType, string name, string address, byte[] deviceData, IEnumerable<NamedSetting> settings)
    {
        builder.Register(c => new DeviceFactoryData(deviceType, name, address, deviceData, settings)).As<IDeviceFactoryData>();
    }

    /// <summary>
    /// Register <typeparamref name="TManager"/> as implementation of <see cref="IBluetoothLEDeviceManager"/> in irder to use it for device discovery
    /// </summary>
    /// <param name="builder">DI container builder</param>
    /// <returns>Registration builder in order to allow additional registration, such as .As<>()</returns>
    public static IRegistrationBuilder<TManager, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterDeviceManager<TManager>(this ContainerBuilder builder)
        where TManager : class, IBluetoothLEDeviceManager
        => builder.RegisterType<TManager>().As<IBluetoothLEDeviceManager>().SingleInstance();
}
