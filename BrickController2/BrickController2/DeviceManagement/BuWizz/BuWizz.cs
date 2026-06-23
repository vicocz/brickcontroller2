using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;

namespace BrickController2.DeviceManagement.BuWizz;

/// <summary>
/// Vendor: BuWizz and all its device types: BuWizz, BuWizz2, BuWizz3 and implementation of IBluetoothLEDeviceManager
/// </summary>
internal class BuWizz : Vendor<BuWizz>
{
    public override string VendorName => "BuWizz";

    protected override void Register(VendorBuilder<BuWizz> builder)
    {
        // classic devices
        builder.ContainerBuilder
            .RegisterDevice<BuWizzDevice>(DeviceType.BuWizz)
            .RegisterDevice<BuWizz3Device>(DeviceType.BuWizz3);

        builder.RegisterDevice<BuWizz2Device>()
            .WithImages("buwizz_image.png", "buwizz_image_small.png");

        // device manager
        builder.RegisterDeviceManager<BuWizzDeviceManager>();
    }
}
