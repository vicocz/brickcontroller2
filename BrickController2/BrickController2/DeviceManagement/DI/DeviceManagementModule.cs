using Autofac;
using BrickController2.DeviceManagement.BuWizz;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.DeviceManagement.Lego;
using BrickController2.DeviceManagement.MouldKing;
using BrickController2.Extensions;
using BrickController2.PlatformServices.BluetoothLE;

namespace BrickController2.DeviceManagement.DI
{
    public class DeviceManagementModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BluetoothDeviceManager>().As<IBluetoothDeviceManager>().SingleInstance();
            builder.RegisterType<InfraredDeviceManager>().As<IInfraredDeviceManager>().SingleInstance();

            builder.RegisterType<DeviceRepository>().As<IDeviceRepository>().SingleInstance();
            builder.RegisterType<DeviceManager>().As<IDeviceManager>().SingleInstance();
            builder.RegisterType<ManualDeviceManager>().As<IManualDeviceManager>().SingleInstance();

            builder.RegisterType<SBrickDevice>().Keyed<Device>(DeviceType.SBrick);
            builder.RegisterType<BuWizzDevice>().Keyed<Device>(DeviceType.BuWizz);
            builder.RegisterType<BuWizz2Device>().Keyed<Device>(DeviceType.BuWizz2);
            builder.RegisterType<BuWizz3Device>().Keyed<Device>(DeviceType.BuWizz3);
            builder.RegisterType<InfraredDevice>().Keyed<Device>(DeviceType.Infrared);
            builder.RegisterType<PoweredUpDevice>().Keyed<Device>(DeviceType.PoweredUp);
            builder.RegisterType<BoostDevice>().Keyed<Device>(DeviceType.Boost);
            builder.RegisterType<TechnicHubDevice>().Keyed<Device>(DeviceType.TechnicHub);
            builder.RegisterType<DuploTrainHubDevice>().Keyed<Device>(DeviceType.DuploTrainHub);
            builder.RegisterType<CircuitCubeDevice>().Keyed<Device>(DeviceType.CircuitCubes);
            builder.RegisterType<Wedo2Device>().Keyed<Device>(DeviceType.WeDo2);
            builder.RegisterType<TechnicMoveDevice>().Keyed<Device>(DeviceType.TechnicMove);
            builder.RegisterType<MK4>().Keyed<Device>(DeviceType.MK4);
            builder.RegisterType<MK6>().Keyed<Device>(DeviceType.MK6);
            builder.RegisterType<MK_DIY>().Keyed<Device>(DeviceType.MK_DIY);
            builder.RegisterType<CaDARaceCar>().Keyed<Device>(DeviceType.CaDA_RaceCar);
            builder.RegisterType<PfxBrickDevice>().Keyed<Device>(DeviceType.PfxBrick);

            builder.Register<DeviceFactory>(c =>
            {
                IComponentContext ctx = c.Resolve<IComponentContext>();
                return (deviceType, name, address, deviceData, settings) => ctx.ResolveOptionalKeyed<Device>(deviceType,
                    new NamedParameter("name", name),
                    new NamedParameter("address", address),
                    new NamedParameter("deviceData", deviceData),
                    new NamedParameter("settings", settings));
            });

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
            builder.RegisterDeviceManager<BuWizzDeviceManager>();
            builder.RegisterDeviceManager<CaDADeviceManager>().As<IBluetoothLEAdvertiserDeviceScanInfo>();
            builder.RegisterDeviceManager<CircuitCubeDeviceManager>();
            builder.RegisterDeviceManager<LegoDeviceManager>();
            builder.RegisterDeviceManager<MouldKingDeviceManager>();
            builder.RegisterDeviceManager<PfxBrickDeviceManager>();
            builder.RegisterDeviceManager<SBrickDeviceManager>();
        }
    }
}
