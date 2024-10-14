using Microsoft.Maui.Hosting;

namespace BrickController2.Extensions;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder ConfigureSymbolFonts(this MauiAppBuilder builder) => builder.ConfigureFonts(fonts =>
    {
        // source: https://github.com/google/material-design-icons/blob/master/font/MaterialIconsOutlined-Regular.otf
        fonts.AddFont("MaterialIconsOutlined-Regular", "Icons");
    });
}
