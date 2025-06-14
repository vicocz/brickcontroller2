using Autofac;
using BrickController2.DeviceManagement.DI;

namespace BrickController2.DeviceManagement.Vendors;

public abstract class Vendor : Module
{
}

public abstract class Vendor<TVendor> : Vendor
    where TVendor : Vendor<TVendor>
{
    public abstract string VendorName { get; }

    protected abstract void Register(VendorBuilder<TVendor> builder);

    protected sealed override void Load(ContainerBuilder builder)
    {
        // do registration of the vendor
        TVendor vendor = (TVendor)this;
        builder.RegisterInstance(vendor);

        // do registration of the vendor
        var vendorBuilder = new VendorBuilder<TVendor>(builder, vendor);
        Register(vendorBuilder);
    }
}