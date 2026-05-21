using Autofac;
using BrickController2.DeviceManagement.CaDA;
using BrickController2.DeviceManagement.MouldKing;
using BrickController2.Droid.PlatformServices.BluetoothLE;
using BrickController2.Droid.PlatformServices.DeviceManagement.CaDA;
using BrickController2.Droid.PlatformServices.DeviceManagement.MouldKing;
using BrickController2.Droid.PlatformServices.GameController;
using BrickController2.Droid.PlatformServices.Infrared;
using BrickController2.Droid.PlatformServices.Localization;
using BrickController2.Droid.PlatformServices.Permission;
using BrickController2.Droid.PlatformServices.SharedFileStorage;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.PlatformServices.Infrared;
using BrickController2.PlatformServices.Localization;
using BrickController2.PlatformServices.Permission;
using BrickController2.PlatformServices.SharedFileStorage;

namespace BrickController2.Droid.PlatformServices.DI
{
    public class PlatformServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InfraredService>().As<IInfraredService>().SingleInstance();
            builder.RegisterType<GameControllerService>().AsSelf().As<IInputDeviceService>().As<IStartable>().SingleInstance(); // ensure it's started as soon as the container is built in Autofac
            builder.RegisterType<BluetoothLEService>().As<IBluetoothLEService>().SingleInstance();
            builder.RegisterType<LocalizationService>().As<ILocalizationService>().SingleInstance();
            builder.RegisterType<SharedFileStorageService>().As<ISharedFileStorageService>().SingleInstance();
            builder.RegisterType<ReadWriteExternalStoragePermission>().As<IReadWriteExternalStoragePermission>().InstancePerDependency();
            builder.RegisterType<BluetoothPermission>().As<IBluetoothPermission>().InstancePerDependency();
            builder.RegisterType<CameraPermission>().As<ICameraPermission>().InstancePerDependency();
            builder.RegisterType<MKPlatformService>().As<IMKPlatformService>().SingleInstance();
            builder.RegisterType<CaDAPlatformService>().As<ICaDAPlatformService>().SingleInstance();
        }
    }
}