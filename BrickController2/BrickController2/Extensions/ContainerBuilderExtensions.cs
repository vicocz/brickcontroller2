using Autofac;
using Autofac.Builder;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.DI;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;
using System.Collections.Generic;

namespace BrickController2.Extensions;

public static class ContainerBuilderExtensions
{
    public static void RegisterDeviceFactory(this ContainerBuilder builder, DeviceType deviceType, string name, string address, byte[]? deviceData = null, IEnumerable<NamedSetting>? settings = null)
    {
        builder.Register(c => new DeviceFactoryData(deviceType, name, address, deviceData ?? [], settings ?? [])).As<IDeviceFactoryData>();
    }

    /// <summary>
    /// Register <typeparamref name="TManager"/> as implementation of <see cref="IBluetoothLEDeviceManager"/> in irder to use it for device discovery
    /// </summary>
    /// <param name="builder">DI container builder</param>
    /// <returns>Registration builder in order to allow additional registration, such as .As<>()</returns>
    public static IRegistrationBuilder<TManager, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterDeviceManager<TManager>(this ContainerBuilder builder)
        where TManager : class, IBluetoothLEDeviceManager
        => builder.RegisterType<TManager>().As<IBluetoothLEDeviceManager>().SingleInstance();

    /// <summary>
    /// Register device of <typeparamref name="TDevice"/> type as a keyed service with its DeviceType.
    /// </summary>
    /// <returns>Registration instance to suppport fluent API</returns>
    internal static DeviceRegistration<TDevice> RegisterDevice<TDevice>(this ContainerBuilder builder, DeviceType deviceType)
        where TDevice : Device
    {
        // register device as a keyed service with its DeviceType
        builder.RegisterType<TDevice>().Keyed<Device>(deviceType);

        return new DeviceRegistration<TDevice>(builder, deviceType);
    }

    /// <summary>
    /// Register device factory for <typeparamref name="TDevice"/> type with the given parameters.
    /// </summary>
    internal static DeviceRegistration<TDevice> WithDeviceFactory<TDevice>(this DeviceRegistration<TDevice> deviceRegistration, string name, string address, byte[]? deviceData = null, IEnumerable<NamedSetting>? settings = null)
        where TDevice : BluetoothAdvertisingDevice
    {
        deviceRegistration.Builder.RegisterDeviceFactory(deviceRegistration.DeviceType, name, address, deviceData, settings);
        return deviceRegistration;
    }
}
