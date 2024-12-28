using BrickController2.DeviceManagement;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BrickController2.UI.ViewModels;

public class DeviceSettingsPageViewModel : PageViewModelBase
{
    private readonly ObservableCollection<DeviceSettingViewModelBase> _settings;
    private readonly ObservableCollection<DeviceSettingGroupViewModel> _groupedSettings;

    public DeviceSettingsPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        IDialogService dialogService,
        NavigationParameters parameters) : base(navigationService, translationService)
    {
        Device = parameters.Get<Device>("device");
        // compose both grouped and ungrouped collections
        _settings = new(Device.CurrentSettings.Select(ToViewModel));
        _groupedSettings = new(_settings
            .OrderBy(x => x.Setting.Name)
            .GroupBy(x => x.Setting.Group)
            .Select(x => new DeviceSettingGroupViewModel(x.Key, x, translationService)));
        // detect grouping
        IsGrouped = _groupedSettings.Any(x => !string.IsNullOrEmpty(x.GroupName));
        DialogService = dialogService;
    }

    public Device Device { get; }
    public IDialogService DialogService { get; }

    public bool IsGrouped { get; }
    public IEnumerable<INotifyPropertyChanged> Settings => IsGrouped ? _groupedSettings : _settings;

    public override async void OnDisappearing()
    {
        base.OnDisappearing();

        // update changed settings on exit
        var changedSettings = _settings
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
        if (setting.IsDoubleType)
        {
            return new DeviceDoubleSettingViewModel(this, setting, TranslationService);
        }        

        throw new InvalidOperationException($"The specified type {setting.Type} is not supported.");
    }
}
