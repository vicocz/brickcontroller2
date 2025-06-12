using Autofac;
using BrickController2.Settings;
using BrickController2.UI.Services.Translation;
using System.Collections.Generic;

namespace BrickController2.DeviceManagement.DI;

/// <summary>
/// Registration class that supports fluent API for registering device factories in the DI container.
/// </summary>
/// <typeparam name="TDevice">Registered device.</typeparam>
/// <param name="builder">DI builder instance</param>
internal class DeviceBuilder<TDevice>(VendorBuilder builder)
    where TDevice : Device, IDeviceType<TDevice>
{
    public ContainerBuilder Builder { get; } = builder.ContainerBuilder;
    public VendorBuilder VendorBuilder { get; } = builder;

    /// <summary>
    /// Register device factory for <typeparamref name="TDevice"/> type with the given parameters.
    /// </summary>
    public DeviceBuilder<TDevice> WithDeviceFactory(string address, byte[]? deviceData = null, IEnumerable<NamedSetting>? settings = null)
    {
        Builder.Register(c =>
        {
            var translation = c.Resolve<ITranslationService>();

            // compose name localized "DeviceType - Address" string
            var name = $"{TDevice.TypeName} {translation.Translate(address)}";
            return new DeviceFactoryData<TDevice>(name, address, deviceData ?? [], settings ?? []);
        }).As<IDeviceFactoryData>();

        return this;
    }

    /// <summary>
    /// Register device factory for <typeparamref name="TDevice"/> type for all provideed <paramref name="addresses"/>
    /// </summary>
    internal DeviceBuilder<TDevice> WithDeviceFactories(params string[] addresses)
    {
        foreach (var address in addresses)
        {
            WithDeviceFactory(address);
        }
        return this;
    }
}
