using BrickController2.UI.Services.Preferences;
using System;
using System.Globalization;

namespace BrickController2.UI.Services.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly IPreferencesService _preferencesService;
    private readonly PlatformServices.Localization.ILocalizationService _localizationService;

    public LocalizationService(IPreferencesService preferencesService, PlatformServices.Localization.ILocalizationService localizationService)
    {
        _preferencesService = preferencesService;
        _localizationService = localizationService;
    }

    public Language CurrentLanguage
    {
        get => _preferencesService.Get("Language", Language.System);

        set
        {
            if (CurrentLanguage != value)
            {
                _preferencesService.Set("Language", value);
                // apply the change
                ApplyCurrentLanguage();
            }
        }
    }

    public void ApplyCurrentLanguage()
    {
        _localizationService.CurrentCultureInfo = CurrentLanguage switch
        {
            Language.English => CultureInfo.GetCultureInfo("en"),
            Language.Deutsch => CultureInfo.GetCultureInfo("de"),
            Language.Magyar => CultureInfo.GetCultureInfo("hu"),

            _ => _localizationService.DefaultCultureInfo
        };
    }
}
