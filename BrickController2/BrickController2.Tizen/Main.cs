using Autofac;
using Autofac.Extensions.DependencyInjection;
using BrickController2.Extensions;
using BrickController2.Tizen.PlatformServices.DI;
using BrickController2.Tizen.UI.CustomRenderers;
using BrickController2.Tizen.UI.Services.DI;
using BrickController2.UI.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace BrickController2.Tizen;

class Program : MauiApplication
{
    protected override MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSymbolFonts()
            .ConfigureMauiHandlers(handlers =>
            {
                handlers
                    .AddHandler<ExtendedSlider, ExtendedSliderHandler>()
                ;
            })
            .ConfigureContainer(new AutofacServiceProviderFactory(), containerBuilder =>
            {
                containerBuilder.RegisterModule<PlatformServicesModule>();
                containerBuilder.RegisterModule<UIServicesModule>();
            });

        return builder.Build();
    }

	static void Main(string[] args)
	{
		var app = new Program();
		app.Run(args);
	}
}
