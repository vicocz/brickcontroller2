using Autofac;
using BrickController2.DeviceManagement.BuWizz;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.DeviceManagement.Lego;
using BrickController2.DeviceManagement.Vendors;
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
            builder.RegisterType<CircuitCubeDevice>().Keyed<Device>(DeviceType.CircuitCubes);
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

            // device managers
            builder.RegisterDeviceManager<BuWizzDeviceManager>();
            builder.RegisterDeviceManager<CaDADeviceManager>().As<IBluetoothLEAdvertiserDeviceScanInfo>();
            builder.RegisterDeviceManager<CircuitCubeDeviceManager>();
            builder.RegisterDeviceManager<PfxBrickDeviceManager>();
            builder.RegisterDeviceManager<SBrickDeviceManager>();

            // execute registration per vendors
            builder.RegisterAssemblyModules<IVendorModule>(typeof(DeviceManagementModule).Assembly);
        }
    }
}
