using BrickController2.PlatformServices.ModelContextProtocol;
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
        private readonly CreationListPageViewModel _parentViewModel;
        private readonly IDialogService _dialogService;
        private readonly IMcpServerService _mcpServerService;

        public SettingsPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDialogService dialogService,
            IThemeService themeService,
            ILocalizationService localizationService,
            IMcpServerService mcpServerService,
            NavigationParameters parameters) :
            base(navigationService, translationService)
        {
            _themeService = themeService;
            _dialogService = dialogService;
            _localizationService = localizationService;
            _mcpServerService = mcpServerService;
            _parentViewModel = parameters.Get<CreationListPageViewModel>("parent");
            SelectThemeCommand = new SafeCommand(SelectThemeAsync);
            SelectLanguageCommand = new SafeCommand(SelectAppLanguageAsync);
            SelectMcpServerPortCommand = new SafeCommand(SelectMcpServerPortAsync);
            SelectMcpServerAuthTokenCommand = new SafeCommand(SelectMcpServerAuthTokenAsync);
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

        public bool CurrentMcpServerEnabled
        {
            get => _mcpServerService.IsMcpServerAvailable && _mcpServerService.IsMcpServerEnabled;
            set
            {
                if (_mcpServerService.IsMcpServerEnabled != value)
                {
                    _mcpServerService.IsMcpServerEnabled= value;
                    RaisePropertyChanged();
                }
            }
        }

        public int CurrentMcpServerPort
        {
            get => _mcpServerService.McpServerPort;
            set
            {
                if (_mcpServerService.McpServerPort != value)
                {
                    _mcpServerService.McpServerPort = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string CurrentMcpServerAuthToken
        {
            get => _mcpServerService.McpServerAuthToken;
            set
            {
                if (_mcpServerService.McpServerAuthToken != value)
                {
                    _mcpServerService.McpServerAuthToken = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool McpServerIsAvailable => _mcpServerService.IsMcpServerAvailable;

        public ICommand SelectThemeCommand { get; }
        public ICommand SelectLanguageCommand { get; }
        public ICommand SelectMcpServerPortCommand { get; }
        public ICommand SelectMcpServerAuthTokenCommand { get; }

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

        private async Task SelectMcpServerPortAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync(
                CurrentMcpServerPort.ToString(),
                $"{McpServerBase.PortDefault}", // McpServerPortDefault 5000
                Translate("Ok"),
                Translate("Cancel"),
                KeyboardType.Numeric,
                (portText) => !string.IsNullOrEmpty(portText) && int.TryParse(portText, out int portNumber) && portNumber >= McpServerBase.PortMin && portNumber <= McpServerBase.PortMax,
                DisappearingToken);

            if (!result.IsOk)
            {
                return;
            }

            if (int.TryParse(result.Result, out int intValue))
            {
                if (intValue < McpServerBase.PortMin || intValue > McpServerBase.PortMax)
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        Translate("ValueOutOfRange"),
                        Translate("Ok"),
                        DisappearingToken);

                    return;
                }

                CurrentMcpServerPort = intValue;
            }
            else
            {
                await _dialogService.ShowMessageBoxAsync(
                    Translate("Warning"),
                    Translate("ValueMustBeNumeric"),
                    Translate("Ok"),
                    DisappearingToken);

                return;
            }
        }

        private async Task SelectMcpServerAuthTokenAsync()
        {
            var result = await _dialogService.ShowInputDialogAsync(
                CurrentMcpServerAuthToken,
                Translate("McpServerAuthTokenValue"),
                Translate("OK"),
                Translate("Cancel"),
                KeyboardType.Text,
                (_) => true,
                DisappearingToken);

            if (result.IsOk)
            {
                CurrentMcpServerAuthToken = result.Result;
            }
        }
    }
}
