using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Localization;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class SettingsPageViewModel : PageViewModelBase
    {
        private readonly IThemeService _themeService;
        private readonly ILocalizationService _localizationService;
        private readonly IDialogService _dialogService;

        public SettingsPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDialogService dialogService,
            IThemeService themeService,
            ILocalizationService localizationService) : 
            base(navigationService, translationService)
        {
            _themeService = themeService;
            _dialogService = dialogService;
            _localizationService = localizationService;

            SelectThemeCommand = new SafeCommand(SelectThemeAsync);
            SelectLanguageCommand = new SafeCommand(SelectAppLanguageAsync);
        }

        public ThemeType CurrentTheme
        {
            get => _themeService.CurrentTheme;
            set
            {
                if (CurrentTheme != value)
                {
                    _themeService.CurrentTheme = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AppLanguage CurrentLanguage
        {
            get => _localizationService.CurrentLanguage;
            set
            {
                if (_localizationService.CurrentLanguage != value)
                {
                    _localizationService.CurrentLanguage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand SelectThemeCommand { get; }
        public ICommand SelectLanguageCommand { get; }

        private async Task SelectThemeAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Enum.GetNames<ThemeType>(),
                Translate("Theme"),
                Translate("Cancel"),
                DisappearingToken);

            if (result.IsOk)
            {
                CurrentTheme = Enum.Parse<ThemeType>(result.SelectedItem);
            }
        }

        private async Task SelectAppLanguageAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Enum.GetNames<AppLanguage>(),
                Translate("Language"),
                Translate("Cancel"),
                DisappearingToken);

            if (result.IsOk)
            {
                CurrentLanguage = Enum.Parse<AppLanguage>(result.SelectedItem);
            }
        }
    }
}
