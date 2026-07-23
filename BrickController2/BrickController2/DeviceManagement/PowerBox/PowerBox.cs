using Autofac;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;

namespace BrickController2.DeviceManagement.PowerBox;

/// <summary>
/// Vendor: PowerBox and all its devices
/// </summary>
internal class PowerBox : Vendor<PowerBox>
{
    public override string VendorName => "PowerBox";

    protected override void Register(VendorBuilder<PowerBox> builder)
    {
        // device manager
        builder.ContainerBuilder.RegisterType<PowerBoxDeviceManager>()
            .SingleInstance();

        // manually added devices
        builder.RegisterDevice<PowerBoxMBattery>()
            .WithDeviceFactory(PowerBoxMBattery.Device, $"{PowerBoxMBattery.TypeName} Device");

    }
}
