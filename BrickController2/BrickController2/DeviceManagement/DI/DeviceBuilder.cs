using Autofac;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Settings;
using BrickController2.UI.Images;
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
    public DeviceBuilder<TVendor, TDevice> WithDeviceFactory(string address, string name, byte[]? deviceData = null, IEnumerable<NamedSetting>? settings = null)
    {
        Builder.Register(c =>
        {
            return new DeviceFactoryData<TVendor, TDevice>(Vendor, name, address, deviceData ?? [], settings ?? []);
        }).As<IDeviceFactoryData>();

        return this;
    }

    /// <summary>
    /// Register custom image resource names for this device type.
    /// </summary>
    public DeviceBuilder<TVendor, TDevice> WithImages(string imageResourceName, string smallImageResourceName)
    {
        Builder.RegisterBuildCallback(scope =>
        {
            var registry = scope.Resolve<IDeviceImageRegistry>();
            registry.Register(TDevice.Type, imageResourceName, smallImageResourceName);
        });

        return this;
    }

    /// <summary>
    /// Register custom image resource name for this device type.
    /// </summary>
    public DeviceBuilder<TVendor, TDevice> WithImage(string imageResourceName) => WithImages(imageResourceName, imageResourceName);
}
