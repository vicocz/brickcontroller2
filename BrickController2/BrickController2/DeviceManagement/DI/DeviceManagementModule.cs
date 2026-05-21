using Autofac;
using BrickController2.DeviceManagement.Vendors;
using BrickController2.Extensions;
using BrickController2.UI.Images;
using System;

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

            builder.RegisterType<InfraredDevice>().Keyed<Device>(DeviceType.Infrared);
            builder.RegisterType<CircuitCubeDevice>().Keyed<Device>(DeviceType.CircuitCubes);
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
            builder.RegisterDeviceManager<CircuitCubeDeviceManager>();
            builder.RegisterDeviceManager<PfxBrickDeviceManager>();

            // execute registration per vendors
            builder.RegisterAssemblyModules<IVendorModule>(typeof(DeviceManagementModule).Assembly);

            // device image registry
            builder.RegisterType<DeviceImageRegistry>().As<IDeviceImageRegistry>().SingleInstance();

            // additional dependencies
            builder.RegisterInstance(Random.Shared);
        }
    }
}
