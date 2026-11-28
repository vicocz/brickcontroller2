// Built-in settings row "Theme": shows the current theme and lets the user pick another one.
using System;
using System.Threading.Tasks;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.Services.Settings;

/// <summary>
/// Picker row for the application theme. Registered in Autofac as <see cref="ISettingsItem"/>.
/// </summary>
public class ThemeSettingsItem : DialogPickerSettingsItem
{
    private readonly IThemeService _themeService;
    private readonly IDialogService _dialogService;
    private readonly ITranslationService _translationService;

    /// <summary>
    /// Creates the row.
    /// </summary>
    public ThemeSettingsItem(IThemeService themeService, IDialogService dialogService, ITranslationService translationService)
        : base(translationService.Translate("Theme"), () => themeService.CurrentTheme.ToString())
    {
        _themeService = themeService;
        _dialogService = dialogService;
        _translationService = translationService;
    }

    /// <inheritdoc />
    public override int Order => 0;

    /// <inheritdoc />
    protected override async Task OnPressedAsync()
    {
        var result = await _dialogService.ShowSelectionDialogAsync(
            Enum.GetNames<ThemeType>(),
            _translationService.Translate("Theme"),
            _translationService.Translate("Cancel"),
            DisappearingToken);

        if (result.IsOk && Enum.TryParse<ThemeType>(result.SelectedItem, out var theme) && _themeService.CurrentTheme != theme)
        {
            _themeService.CurrentTheme = theme;
            RaiseValueChanged();
        }
    }
}
