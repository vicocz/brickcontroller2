using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BrickController2.UWP
{
    public sealed partial class MainPage
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
