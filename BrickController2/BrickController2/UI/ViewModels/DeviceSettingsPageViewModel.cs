using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Collections.ObjectModel;
using System.Linq;

namespace BrickController2.UI.ViewModels
{
    public class DeviceSettingsPageViewModel : PageViewModelBase
    {
        public DeviceSettingsPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            NavigationParameters parameters) : 
            base(navigationService, translationService)
        {
            Device = parameters.Get<Device>("device");
            Settings = new ObservableCollection<DeviceSettingViewModel>(Device.CurrentSettings.Select(setting => new DeviceSettingViewModel(setting, translationService)));
        }

        public Device Device { get; }

        public ObservableCollection<DeviceSettingViewModel> Settings { get; }

        public override async void OnDisappearing()
        {
            // update changed settings on exit
            var changedSettings = Settings
                .Where(s => s.HasChanged)
                .Select(s => s.Setting)
                .ToArray();

            if (changedSettings.Any())
                await Device.UpdateDeviceSettingsAsync(changedSettings);
        }
    }
}
