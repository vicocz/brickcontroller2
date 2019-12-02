using Autofac;
using BrickController2.Windows.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.BluetoothLE;
using BrickController2.PlatformServices.GameController;
using BrickController2.PlatformServices.Infrared;
using BrickController2.PlatformServices.Localization;
using BrickController2.PlatformServices.Preferences;
using BrickController2.PlatformServices.Versioning;
using BrickController2.Windows.PlatformServices.Infrared;
using BrickController2.Windows.PlatformServices.Versioning;
using BrickController2.Windows.PlatformServices.Localization;
using BrickController2.Windows.PlatformServices.GameController;

namespace BrickController2.Windows.PlatformServices.DI
{
    public class PlatformServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InfraredService>().As<IInfraredService>().SingleInstance();
            builder.RegisterType<GameControllerService>().AsSelf().As<IGameControllerService>().SingleInstance();
            builder.RegisterType<VersionService>().As<IVersionService>().SingleInstance();
            builder.RegisterType<BleService>().As<IBluetoothLEService>().SingleInstance();
            builder.RegisterType<LocalizationService>().As<ILocalizationService>().SingleInstance();
            builder.RegisterType<Preferences.Preferences>().As<IPreferences>().SingleInstance();
        }
    }
}