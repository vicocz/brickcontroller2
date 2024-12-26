using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BrickController2.UI.ViewModels;

public class DeviceSettingsPageViewModel : PageViewModelBase
{
    public DeviceSettingsPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        IDialogService dialogService,
        NavigationParameters parameters) : base(navigationService, translationService)
    {
        Device = parameters.Get<Device>("device");
        Settings = new ObservableCollection<DeviceSettingViewModelBase>(Device.CurrentSettings.Select(ToViewModel));
        DialogService = dialogService;
    }

    public Device Device { get; }
    public IDialogService DialogService { get; }

    public ObservableCollection<DeviceSettingViewModelBase> Settings { get; }

    public override async void OnDisappearing()
    {
        base.OnDisappearing();

        // update changed settings on exit
        var changedSettings = Settings
            .Where(s => s.HasChanged)
            .Select(s => s.Setting)
            .ToArray();

        if (changedSettings.Any())
            await Device.UpdateDeviceSettingsAsync(changedSettings);
    }

    private DeviceSettingViewModelBase ToViewModel(DeviceSetting setting)
    {
        if (setting.IsBoolType)
        {
            return new DeviceBoolSettingViewModel(this, setting, TranslationService);
        }
        if (setting.IsEnumType)
        {
            return new DeviceEnumSettingViewModel(this, setting, TranslationService);
        }
        throw new InvalidOperationException($"The specified type {setting.Type} is not supported.");
    }
}
