using Autofac;
using BrickController2.DeviceManagement.DI;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;
using BrickController2.PlatformServices.InputDeviceService;

namespace BrickController2.DeviceManagement.Lego;

/// <summary>
/// Vendor: LEGO and all it's device and implementation of IBluetoothLEDeviceManager
/// </summary>
internal class Lego : Vendor<Lego>
{
    public override string VendorName => "LEGO";

    protected override void Register(VendorBuilder<Lego> builder)
    {
        // classic devices
        builder.ContainerBuilder
            .RegisterDevice<PoweredUpDevice>(DeviceType.PoweredUp)
            .RegisterDevice<BoostDevice>(DeviceType.Boost)
            .RegisterDevice<TechnicHubDevice>(DeviceType.TechnicHub)
            .RegisterDevice<DuploTrainHubDevice>(DeviceType.DuploTrainHub)
            .RegisterDevice<Wedo2Device>(DeviceType.WeDo2)
            .RegisterDevice<TechnicMoveDevice>(DeviceType.TechnicMove);

        // input devices
        builder.ContainerBuilder.RegisterDevice<LegoRemoteControl>(DeviceType.LegoRemoteControl);
        builder.ContainerBuilder.RegisterType<LegoControllerService>().AsSelf().As<IInputDeviceService>().As<IStartable>().SingleInstance(); // ensure it's started as soon as the container is built in Autofac

        // device manager
        builder.RegisterDeviceManager<LegoDeviceManager>();
    }
}
