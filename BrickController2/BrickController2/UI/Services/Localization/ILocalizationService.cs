using System;

namespace BrickController2.UI.Services.Localization;

public interface ILocalizationService
{
    AppLanguage CurrentLanguage { get; set; }
    void ApplyCurrentLanguage();

    public event EventHandler<AppLanguage>? LanguageChanged;
}
