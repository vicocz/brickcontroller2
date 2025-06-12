using Autofac;
using Autofac.Builder;
using BrickController2.DeviceManagement.DI;
using BrickController2.Extensions;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.Vendors;

internal abstract class Vendor : Module
{
    protected abstract string VendorName { get; }

    protected virtual void RegisterDevices(VendorBuilder registration)
    {
        // empty method to be overridden by derived classes
    }

    protected override void Load(ContainerBuilder builder)
    {
        // do registration of the vendor
        var vendorBuilder = new VendorBuilder(builder, this);
        RegisterDevices(vendorBuilder);
    }
}

internal abstract class Vendor<TManager> : Vendor
    where TManager : class, IBluetoothLEDeviceManager
{
    protected virtual IRegistrationBuilder<TManager, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterManager(ContainerBuilder builder)
        => builder.RegisterDeviceManager<TManager>();

    protected sealed override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        // do registration of the manager
        RegisterManager(builder);
    }
}
