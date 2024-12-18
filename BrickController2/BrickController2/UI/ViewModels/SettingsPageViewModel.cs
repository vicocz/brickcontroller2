using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
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
        private readonly IDialogService _dialogService;

        public SettingsPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IDialogService dialogService,
            IThemeService themeService) : 
            base(navigationService, translationService)
        {
            _themeService = themeService;
            _dialogService = dialogService;

            SelectThemeCommand = new SafeCommand(async () => await SelectThemeAsync());
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

        public ICommand SelectThemeCommand { get; }

        private async Task SelectThemeAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Enum.GetNames(typeof(ThemeType)),
                Translate("Theme"),
                Translate("Cancel"),
                DisappearingToken);

            if (result.IsOk)
            {
                CurrentTheme = (ThemeType)Enum.Parse(typeof(ThemeType), result.SelectedItem);
            }
        }
    }
}
