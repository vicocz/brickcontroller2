using Autofac;
using Autofac.Builder;
using BrickController2.DeviceManagement;
using BrickController2.DeviceManagement.Lego;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.InputDeviceService;

namespace BrickController2.Extensions;

public static class ContainerBuilderExtensions
{
    /// <summary>
    /// Register <typeparamref name="TManager"/> as implementation of <see cref="IBluetoothLEDeviceManager"/> in order to use it for device discovery
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
    internal static ContainerBuilder RegisterDevice<TDevice>(this ContainerBuilder builder, DeviceType deviceType)
        where TDevice : Device
    {
        // register device as a keyed service with its DeviceType
        builder.RegisterType<TDevice>().Keyed<Device>(deviceType);
        return builder;
    }

    /// <summary>
    /// Register input device service of <typeparamref name="TInputDeviceService"/> type.
    /// </summary>
    /// <returns>Registration instance to support fluent API</returns>
    internal static ContainerBuilder RegisterInputDeviceService<TInputDeviceService>(this ContainerBuilder builder)
        where TInputDeviceService : IInputDeviceService, IStartable
    {
        builder.RegisterType<TInputDeviceService>()
            .As<IInputDeviceService>()
            .As<IStartable>()
            .SingleInstance(); // ensure it's started as soon as the container is built in Autofac
        return builder;
    }
}
