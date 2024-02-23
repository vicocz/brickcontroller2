using Autofac;
using BrickController2.BusinessLogic.DI;
using BrickController2.CreationManagement.DI;
using BrickController2.Database.DI;
using BrickController2.DeviceManagement.DI;
using BrickController2.DI;
using BrickController2.iOS.PlatformServices.DI;
using BrickController2.iOS.UI.CustomRenderers;
using BrickController2.iOS.UI.Services.DI;
using BrickController2.UI.Controls;
using BrickController2.UI.DI;
using Foundation;
using UIKit;

namespace BrickController2.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : MauiUIApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication uiApp, NSDictionary options)
        {
            // Preventing screen turning off
            UIApplication.SharedApplication.IdleTimerDisabled = true;

            return base.FinishedLaunching(uiApp, options);
        }

        protected override MauiApp CreateMauiApp() => ApplicationBuilder.Create()
            // per platform handlers
            .ConfigureMauiHandlers(handlers =>
            {
                handlers
                    .AddHandler<ExtendedSlider, ExtendedSliderRenderer>()
                    .AddHandler<ColorImage, ColorImageRenderer>()
                    .AddHandler<ListView, NoAnimListViewRenderer>()
                ;
            })
            .ConfigureContainer((containerBuilder) =>
            {
                containerBuilder.RegisterModule<PlatformServicesModule>();
                containerBuilder.RegisterModule<UIServicesModule>();
            })
            // finally build
            .Build()
            ;
    }
}
