using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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

        SaveSettingsCommand = new SafeCommand(ApplyChanges, () => _settings.Any(x => x.HasChanged));
        ResetToDefaultsCommand = new SafeCommand(ResetToDefaults, () => _settings.Any(x => x.HasNonDefaultValue));
    }

    public ICommand SaveSettingsCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }

    public Device Device { get; }
    public IDialogService DialogService { get; }

    public bool IsGrouped { get; }
    public IEnumerable<INotifyPropertyChanged> Settings => IsGrouped ? _groupedSettings : _settings;

    internal void OnSettingChanged()
    {
        SaveSettingsCommand.RaiseCanExecuteChanged();
        ResetToDefaultsCommand.RaiseCanExecuteChanged();
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

    private async Task ApplyChanges()
    {
        try
        {
            await DialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    var changedSettings = _settings
                        .Where(s => s.HasChanged)
                        .Select(s => s.Setting)
                        .ToArray();

                    await Device.UpdateDeviceSettingsAsync(changedSettings);
                },
                Translate("Saving"),
                token: DisappearingToken);

            await NavigationService.NavigateBackAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ResetToDefaults()
    {
        foreach (var setting in _settings.Where(s => s.HasNonDefaultValue))
        {
            setting.ResetToDefault();
        }
    }
}
