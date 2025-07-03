using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;

namespace BrickController2.DeviceManagement.MouldKing;

/// <summary>
/// Vendor: Mould King and all it's device and implementation of IBluetoothLEDeviceManager
/// </summary>
internal class MouldKing : Vendor<MouldKing>
{
    public override string VendorName => "Mould King";

    protected override void Register(VendorBuilder<MouldKing> builder)
    {
        // classic devices
        builder.ContainerBuilder.RegisterDevice<MK_DIY>(DeviceType.MK_DIY);

        // manually added devices
        builder.RegisterDevice<MK4>()
            .WithDeviceFactory(MK4.Device1, $"{MK4.TypeName} Device 1")
            .WithDeviceFactory(MK4.Device2, $"{MK4.TypeName} Device 2")
            .WithDeviceFactory(MK4.Device3, $"{MK4.TypeName} Device 3");

        builder.RegisterDevice<MK5>()
            .WithDeviceFactory(MK5.Device, MK5.TypeName);

        builder.RegisterDevice<MK6>()
            .WithDeviceFactory(MK6.Device1, $"{MK6.TypeName} Device 1")
            .WithDeviceFactory(MK6.Device2, $"{MK6.TypeName} Device 2")
            .WithDeviceFactory(MK6.Device3, $"{MK6.TypeName} Device 3");

        // device manager
        builder.RegisterDeviceManager<MouldKingDeviceManager>();
    }
}
