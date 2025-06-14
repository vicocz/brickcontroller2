using Autofac;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Settings;
using BrickController2.UI.Services.Translation;
using System.Collections.Generic;

namespace BrickController2.DeviceManagement.DI;

/// <summary>
/// Registration class that supports fluent API for registering devices and factories 
/// of given <typeparamref name="TVendor"/> in the DI container.
/// </summary>
/// <typeparam name="TDevice">Registered device.</typeparam>
/// <param name="builder">DI builder instance</param>
public class DeviceBuilder<TVendor, TDevice>(VendorBuilder<TVendor> builder)
    where TDevice : Device, IDeviceType<TDevice>
    where TVendor : Vendor<TVendor>
{
    public ContainerBuilder Builder { get; } = builder.ContainerBuilder;
    public TVendor Vendor { get; } = builder.Vendor;

    /// <summary>
    /// Register device factory for <typeparamref name="TDevice"/> type with the given parameters.
    /// </summary>
    public DeviceBuilder<TVendor, TDevice> WithDeviceFactory(string address, byte[]? deviceData = null, IEnumerable<NamedSetting>? settings = null)
    {
        Builder.Register(c =>
        {
            var translation = c.Resolve<ITranslationService>();

            // compose name localized "DeviceType - Address" string
            var name = $"{TDevice.TypeName} {translation.Translate(address)}";
            return new DeviceFactoryData<TVendor, TDevice>(Vendor, name, address, deviceData ?? [], settings ?? []);
        }).As<IDeviceFactoryData>();

        return this;
    }

    /// <summary>
    /// Register device factory for <typeparamref name="TDevice"/> type for all provideed <paramref name="addresses"/>
    /// </summary>
    internal DeviceBuilder<TVendor, TDevice> WithDeviceFactories(params string[] addresses)
    {
        foreach (var address in addresses)
        {
            WithDeviceFactory(address);
        }
        return this;
    }
}
