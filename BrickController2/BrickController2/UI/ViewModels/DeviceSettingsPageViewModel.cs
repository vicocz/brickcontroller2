using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System.Collections.ObjectModel;
using Device = BrickController2.DeviceManagement.Device;

namespace BrickController2.UI.ViewModels;

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
        // update if any change on exit
        if (Settings.Any(s => s.HasChanged))
        {
            await Device.UpdateDeviceSettingsAsync(Settings.Select(s => s.KeyValuePair));
        }
    }
}
