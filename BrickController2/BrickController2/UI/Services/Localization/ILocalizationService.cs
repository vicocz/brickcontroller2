namespace BrickController2.UI.Services.Localization;

public interface ILocalizationService
{
    Language CurrentLanguage { get; set; }
    void ApplyCurrentLanguage();
}
