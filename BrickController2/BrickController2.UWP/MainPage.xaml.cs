using Autofac;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.UI.DI;
using BrickController2.Windows.PlatformServices.DI;
using BrickController2.Windows.PlatformServices.GameController;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace BrickController2.Windows
{
    public sealed partial class MainPage
    {
        private readonly GameControllerService _gameControllerService;
        private readonly IContainer _container;

        public MainPage()
        {
            this.InitializeComponent();

            // override system settings
            var appView = ApplicationView.GetForCurrentView();
            appView.TitleBar.ButtonBackgroundColor = Colors.White;
            appView.TitleBar.ButtonPressedBackgroundColor = appView.TitleBar.ButtonHoverBackgroundColor = Colors.Red;

            _container = InitDI();
            _gameControllerService = _container.Resolve<GameControllerService>();

            base.LoadApplication(_container.Resolve<BrickController2.App>());

            // ensure GameControllerService is properly linked
            _gameControllerService.InitializeComponent(Window.Current.CoreWindow);
        }

        private static IContainer InitDI()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PlatformServicesModule());

            builder.RegisterModule(new BusinessLogicModule());
            builder.RegisterModule(new DatabaseModule());
            builder.RegisterModule(new CreationManagementModule());
            builder.RegisterModule(new DeviceManagementModule());
            builder.RegisterModule(new UiModule());

            return builder.Build();
        }
    }
}
