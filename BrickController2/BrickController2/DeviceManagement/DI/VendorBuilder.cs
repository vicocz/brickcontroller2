using Autofac;
using Autofac.Builder;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.DI;

/// <summary>
/// Registration class that supports fluent API for building of vendor's device(s)
/// </summary>
/// <param name="builder">DI builder instance</param>
public class VendorBuilder<TVendor>(ContainerBuilder builder, TVendor vendor)
    where TVendor : Vendor<TVendor>
{
    public ContainerBuilder ContainerBuilder { get; } = builder;
    public TVendor Vendor { get; } = vendor;

    /// <summary>
    /// Register device of <typeparamref name="TDevice"/> type as a keyed service with its DeviceType.
    /// </summary>
    /// <returns>Registration instance to suppport fluent API</returns>
    public DeviceBuilder<TVendor, TDevice> RegisterDevice<TDevice>()
        where TDevice : Device, IDeviceType<TDevice>
    {
        // register device as a keyed service with its DeviceType
        ContainerBuilder.RegisterDevice<TDevice>(TDevice.Type);

        return new DeviceBuilder<TVendor,TDevice>(this);
    }

    /// <summary>
    /// Register <typeparamref name="TManager"/> as implementation of <see cref="IBluetoothLEDeviceManager"/> 
    /// </summary>
    /// <returns>Registration builder in order to allow additional registration, such as .As<>()</returns>
    public IRegistrationBuilder<TManager, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterDeviceManager<TManager>()
        where TManager : class, IBluetoothLEDeviceManager
        => ContainerBuilder.RegisterDeviceManager<TManager>();
}
