using Autofac;

namespace BrickController2.DeviceManagement.DI;

/// <summary>
/// Registration class that supports fluent API for registering device factories in the DI container.
/// </summary>
/// <typeparam name="TDevice">Registered device.</typeparam>
/// <param name="builder">DI builder instance</param>
internal class DeviceRegistration<TDevice>(ContainerBuilder builder, DeviceType deviceType)
    where TDevice : Device
{
    public ContainerBuilder Builder { get; } = builder;
    public DeviceType DeviceType { get; } = deviceType;
}