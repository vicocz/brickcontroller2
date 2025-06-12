using Autofac;
using BrickController2.DeviceManagement.DI;
using BrickController2.Extensions;

namespace BrickController2.DeviceManagement.MouldKing;

public class MouldKingModule : Module, IVendorModule
{
    protected override void Load(ContainerBuilder builder)
    {
        // clasic devices
        builder.RegisterDevice<MK_DIY>(DeviceType.MK_DIY);

        // manually added devices
        builder.RegisterDevice<MK4>(DeviceType.MK4)
            .WithDeviceFactory(MK4.Device1)
            .WithDeviceFactory(MK4.Device2)
            .WithDeviceFactory(MK4.Device3);

        builder.RegisterDevice<MK6>(DeviceType.MK6)
            .WithDeviceFactory(MK6.Device1)
            .WithDeviceFactory(MK6.Device2)
            .WithDeviceFactory(MK6.Device3);

        // device managers
        builder.RegisterDeviceManager<MouldKingDeviceManager>();
    }
}
