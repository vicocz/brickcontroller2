using BrickController2.Tizen.PlatformServices.DI;
using BrickController2.Tizen.UI.CustomRenderers;
using BrickController2.Tizen.UI.Services.DI;

namespace BrickController2.Tizen;

class Program : MauiApplication
{
    protected override MauiApp CreateMauiApp() => ApplicationBuilder.Create()
        // per platform handlers
        .ConfigureMauiHandlers(handlers =>
        {
            handlers
                .AddHandler<ExtendedSlider, ExtendedSliderHandler>()
            ;
        })
        .ConfigureContainer((containerBuilder) =>
        {
            containerBuilder.Register<Android.Content.Context>((c) => Android.App.Application.Context).SingleInstance();
            containerBuilder.RegisterModule<PlatformServicesModule>();
            containerBuilder.RegisterModule<UIServicesModule>();
        })
        // finally build
        .Build()
        ;

	static void Main(string[] args)
	{
		var app = new Program();
		app.Run(args);
	}
}
