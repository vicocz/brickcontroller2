using Autofac;
using BrickController2.DI;
using BrickController2.Tizen.PlatformServices.DI;
using BrickController2.Tizen.UI.Services.DI;

namespace BrickController2.Tizen;

internal class Program : MauiApplication
{
    protected override MauiApp CreateMauiApp() => ApplicationBuilder.Create()
        .ConfigureContainer((containerBuilder) =>
        {
            containerBuilder.RegisterModule<PlatformServicesModule>();
            containerBuilder.RegisterModule<UIServicesModule>();
        })
        // finally build
        .Build();

    static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}
