using BrickController2.PlatformServices.Localization;
using Microsoft.Maui.Controls;
using System.Globalization;
using System.Threading;

[assembly: Dependency(typeof(BrickController2.Windows.PlatformServices.Localization.LocalizationService))]
namespace BrickController2.Windows.PlatformServices.Localization;

public class LocalizationService : ILocalizationService
{
    private CultureInfo? _cultureInfo;

    public CultureInfo CurrentCultureInfo
    {
        get => _cultureInfo ?? DefaultCultureInfo;

        set
        {
            _cultureInfo = value;
            CultureInfo.CurrentUICulture = value;
            Thread.CurrentThread.CurrentCulture = value;
            Thread.CurrentThread.CurrentUICulture = value;
        }
    }

    public CultureInfo DefaultCultureInfo => CultureInfo.InstalledUICulture;
}