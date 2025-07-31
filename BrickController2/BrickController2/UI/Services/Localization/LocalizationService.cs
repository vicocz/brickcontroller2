using BrickController2.UI.Services.Preferences;
using System;
using System.Globalization;

namespace BrickController2.UI.Services.Localization;

public class LocalizationService : ILocalizationService
{
    private readonly IPreferencesService _preferencesService;
    private readonly PlatformServices.Localization.ILocalizationService _localizationService;

    // Define the event
    public event EventHandler<AppLanguage>? LanguageChanged;

    public LocalizationService(IPreferencesService preferencesService, PlatformServices.Localization.ILocalizationService localizationService)
    {
        _preferencesService = preferencesService;
        _localizationService = localizationService;
    }

    public AppLanguage CurrentLanguage
    {
        get => _preferencesService.Get("Language", AppLanguage.System);

        set
        {
            if (CurrentLanguage != value)
            {
                _preferencesService.Set("Language", value);
                // apply the change
                ApplyCurrentLanguage();
                // trigger event for UI updates
                LanguageChanged?.Invoke(this, value);
            }
        }
    }

    public void ApplyCurrentLanguage()
    {
        _localizationService.CurrentCultureInfo = CurrentLanguage switch
        {
            AppLanguage.English => CultureInfo.GetCultureInfo("en"),
            AppLanguage.Deutsch => CultureInfo.GetCultureInfo("de"),
            AppLanguage.Magyar => CultureInfo.GetCultureInfo("hu"),

            _ => _localizationService.DefaultCultureInfo
        };
    }
}
