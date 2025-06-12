using Autofac;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;

namespace BrickController2.DeviceManagement.DI;

/// <summary>
/// Registration class that supports fluent API for building of vendor's device(s)
/// </summary>
/// <param name="builder">DI builder instance</param>
internal class VendorBuilder(ContainerBuilder builder, Vendor vendor)
{
    public ContainerBuilder ContainerBuilder { get; } = builder;
    public Vendor Vendor { get; } = vendor;

    /// <summary>
    /// Register device of <typeparamref name="TDevice"/> type as a keyed service with its DeviceType.
    /// </summary>
    /// <returns>Registration instance to suppport fluent API</returns>
    public DeviceBuilder<TDevice> RegisterDevice<TDevice>()
        where TDevice : Device, IDeviceType<TDevice>
    {
        // register device as a keyed service with its DeviceType
        ContainerBuilder.RegisterDevice<TDevice>(TDevice.Type);

        return new DeviceBuilder<TDevice>(this);
    }
}