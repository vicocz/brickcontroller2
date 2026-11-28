// Built-in settings row "Language": picks the language and reloads the UI so the change takes effect.
using System;
using System.Threading.Tasks;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Localization;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.Services.Settings;

/// <summary>
/// Picker row for the application language. Registered in Autofac as <see cref="ISettingsItem"/>.
/// </summary>
public class LanguageSettingsItem : DialogPickerSettingsItem
{
    private const int ProgressDialogDelayMs = 500;

    private readonly ILocalizationService _localizationService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly ITranslationService _translationService;
    private readonly Action _reloadRootPage;

    /// <summary>
    /// Creates the row.
    /// </summary>
    /// <param name="reloadRootPage">Recreates the root page so the new language is applied (App.ReloadRootPage in the app).</param>
    public LanguageSettingsItem(
        ILocalizationService localizationService,
        IDialogService dialogService,
        INavigationService navigationService,
        ITranslationService translationService,
        Action reloadRootPage)
        : base(translationService.Translate("Language"), () => localizationService.CurrentLanguage.ToString())
    {
        _localizationService = localizationService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _translationService = translationService;
        _reloadRootPage = reloadRootPage;
    }

    /// <inheritdoc />
    public override int Order => 10;

    /// <inheritdoc />
    protected override async Task OnPressedAsync()
    {
        var result = await _dialogService.ShowSelectionDialogAsync(
            Enum.GetNames<Language>(),
            _translationService.Translate("Language"),
            _translationService.Translate("Cancel"),
            DisappearingToken);

        if (!result.IsOk || !Enum.TryParse<Language>(result.SelectedItem, out var language))
        {
            return;
        }

        if (_localizationService.CurrentLanguage != language)
        {
            _localizationService.CurrentLanguage = language;
            RaiseValueChanged();
        }

        // notify via progress dialog while the root page is recreated to apply the change
        await _dialogService.ShowProgressDialogAsync(
            false,
            (progressDialog, token) =>
            {
                _reloadRootPage();
                // some delay to show the progress dialog
                return Task.Delay(ProgressDialogDelayMs, token);
            },
            _translationService.Translate("Applying"),
            token: DisappearingToken);

        // back to the previous page; the root page view model refreshes its settings command when it appears again
        await _navigationService.NavigateBackAsync();
    }
}
