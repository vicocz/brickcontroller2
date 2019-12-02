using Autofac;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.UI.DI;
using BrickController2.Windows.PlatformServices.DI;
using BrickController2.Windows.PlatformServices.GameController;
using BrickController2.Windows.UI.Services.DI;
using Windows.UI.Xaml;

namespace BrickController2.Windows
{
    public sealed partial class MainPage : Xamarin.Forms.Platform.UWP.WindowsPage
    {
        private readonly GameControllerService _gameControllerService;
        private readonly IContainer _container;

        public MainPage()
        {
            this.InitializeComponent();

            _container = InitDI();
            _gameControllerService = _container.Resolve<GameControllerService>();

            base.LoadApplication(_container.Resolve<App>());

            // ensure GameControllerService is properly linked
            _gameControllerService.InitializeComponent(Window.Current.CoreWindow);
        }

        private static IContainer InitDI()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PlatformServicesModule());
            builder.RegisterModule(new UIServicesModule());

            builder.RegisterModule(new DatabaseModule());
            builder.RegisterModule(new CreationManagementModule());
            builder.RegisterModule(new DeviceManagementModule());
            builder.RegisterModule(new UiModule());

            return builder.Build();
        }
    }
}
