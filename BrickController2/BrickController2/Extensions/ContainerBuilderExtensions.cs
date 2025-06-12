using Autofac;
using Autofac.Builder;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.DI;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.Settings;
using BrickController2.UI.Services.Translation;
using System.Collections.Generic;

namespace BrickController2.Extensions;

public static class ContainerBuilderExtensions
{
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
    internal static DeviceRegistration<TDevice> WithDeviceFactory<TDevice>(this DeviceRegistration<TDevice> deviceRegistration, string address, byte[]? deviceData = null, IEnumerable<NamedSetting>? settings = null)
        where TDevice : BluetoothAdvertisingDevice
    {
        deviceRegistration.Builder.Register(c =>
        {
            var translation = c.Resolve<ITranslationService>();

            // compose name localized "DeviceType - Address" string
            var name = $"{translation.Translate(deviceRegistration.DeviceType.ToString())} {translation.Translate(address)}";
            return new DeviceFactoryData(deviceRegistration.DeviceType, name, address, deviceData ?? [], settings ?? []);
        }).As<IDeviceFactoryData>();

        return deviceRegistration;
    }
}
