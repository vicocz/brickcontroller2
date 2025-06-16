using Autofac;
using Autofac.Core;
using BrickController2.DeviceManagement.DI;

namespace BrickController2.DeviceManagement.Vendors;

/// <summary>
/// Base class for a vendor to register all required devices and dependencies
/// </summary>
public abstract class Vendor<TVendor> : Module, IVendorModule
    where TVendor : Vendor<TVendor>
{
    public abstract string VendorName { get; }

    protected abstract void Register(VendorBuilder<TVendor> builder);

    protected sealed override void Load(ContainerBuilder builder)
    {
        // do registration of the vendor itself
        TVendor vendor = (TVendor)this;
        builder.RegisterInstance(vendor);

        // do registration of the vendor
        var vendorBuilder = new VendorBuilder<TVendor>(builder, vendor);
        Register(vendorBuilder);
    }
}

public interface IVendorModule : IModule
{
}
