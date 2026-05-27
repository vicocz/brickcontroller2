using BrickController2.InputDeviceManagement.Sensors;
using BrickController2.PlatformServices.InputDeviceService;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Localization;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class SettingsPageViewModel : PageViewModelBase
    {
        private const int ProgressDialogDelayMs = 500;

        private readonly IThemeService _themeService;
        private readonly ILocalizationService _localizationService;
        private readonly IInputDeviceService<OrientationSensorController> _orientationSensorService;
        private readonly CreationListPageViewModel _parentViewModel;
        private readonly IDialogService _dialogService;

        public SettingsPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDialogService dialogService,
            IThemeService themeService,
            ILocalizationService localizationService,
            IInputDeviceService<OrientationSensorController> orientationSensorService,
            NavigationParameters parameters) : 
            base(navigationService, translationService)
        {
            _themeService = themeService;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _orientationSensorService = orientationSensorService;
            _parentViewModel = parameters.Get<CreationListPageViewModel>("parent");
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

        public Language CurrentLanguage
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

        public bool IsOrientationSensorSupported => _orientationSensorService.IsSupported;

        public bool IsOrientationSensorEnabled
        {
            get => _orientationSensorService.IsEnabled;
            set
            {
                if (IsOrientationSensorEnabled != value)
                {
                    _orientationSensorService.IsEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

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
                Enum.GetNames<Language>(),
                Translate("Language"),
                Translate("Cancel"),
                DisappearingToken);

            if (result.IsOk && Enum.TryParse<Language>(result.SelectedItem, out var currentLanguage))
            {
                // apply the change
                CurrentLanguage = currentLanguage;

                // use some notification via progress dialog
                await _dialogService.ShowProgressDialogAsync(
                    false,
                    (progressDialog, token) =>
                    {
                        // recreate the root page to apply the change
                        if (Application.Current is App myApp)
                        {
                            myApp.ReloadRootPage();
                        }
                        // some delay to show the progress dialog
                        return Task.Delay(ProgressDialogDelayMs, token);
                    },
                    Translate("Applying"),
                    token: DisappearingToken);

                // back to the previous page
                await NavigationService.NavigateBackAsync();
                // workaround for settings cmd available
                _parentViewModel.OpenSettingsPageCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
