using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Vendor: Mould King and all it's device and implementation of IBluetoothLEDeviceManager
/// </summary>
internal class MouldKing : Vendor<MouldKingDeviceManager>
{
    protected override string VendorName => "Mould King";

    protected override void RegisterDevices(VendorBuilder builder)
    {
        // clasic devices
        builder.ContainerBuilder.RegisterDevice<MK_DIY>(DeviceType.MK_DIY);

        // manually added devices
        builder.RegisterDevice<MK4>()
            .WithDeviceFactories(MK4.Device1, MK4.Device2, MK4.Device3);

        builder.RegisterDevice<MK6>()
            .WithDeviceFactories(MK6.Device1, MK6.Device2, MK6.Device3);
    }
}
